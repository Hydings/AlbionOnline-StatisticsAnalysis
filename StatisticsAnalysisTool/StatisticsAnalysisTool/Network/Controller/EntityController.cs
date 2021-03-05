﻿using log4net;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Controller
{
    public class EntityController
    {
        private readonly TrackingController _trackingController;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ConcurrentDictionary<long, GameObject> _knownEntities = new ConcurrentDictionary<long, GameObject>();

        public EntityController(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public void AddEntity(long objectId, string name, GameObjectType objectType, GameObjectSubType objectSubType, bool isInParty = false)
        {
            if (_knownEntities.ContainsKey(objectId))
            {
                return;
            }

            var gameObject = new GameObject(objectId)
            {
                Name = name,
                ObjectType = objectType,
                ObjectSubType = objectSubType,
                IsInParty = isInParty
            };

            _knownEntities.TryAdd(objectId, gameObject);
            OnAddEntity?.Invoke(gameObject);
        }

        public void RemoveEntity(long objectId)
        {
            _knownEntities.TryRemove(objectId, out var gameObject);
        }

        public void RemoveAll()
        {
            _knownEntities.Clear();
        }

        public void SetInParty(string username)
        {
            var userObject = _knownEntities.FirstOrDefault(x => x.Value.Name == username);
            if (userObject.Value != null)
            {
                userObject.Value.IsInParty = true;
            }
        }

        public void SetInParty(List<string> usernames)
        {
            foreach (var username in usernames)
            {
                SetInParty(username);
            }
        }

        public KeyValuePair<long, GameObject>? GetEntity(long objectId)
        {
            return _knownEntities?.FirstOrDefault(x => x.Key == objectId);
        }

        public IEnumerable<GameObject> GetEntities()
        {
            return new List<GameObject>(_knownEntities.Values);
        }

        public event Action<GameObject> OnAddEntity;

        public void HealthUpdate(
            long objectId,
            GameTimeStamp TimeStamp,
            double HealthChange,
            double NewHealthValue,
            EffectType EffectType,
            EffectOrigin EffectOrigin,
            long CauserId,
            int CausingSpellType
        )
        {
            OnHealthUpdate?.Invoke(
                objectId,
                TimeStamp,
                HealthChange,
                NewHealthValue,
                EffectType,
                EffectOrigin,
                CauserId,
                CausingSpellType
            );
        }

        public event Action<long, GameTimeStamp, double, double, EffectType, EffectOrigin, long, int> OnHealthUpdate;
    }
}