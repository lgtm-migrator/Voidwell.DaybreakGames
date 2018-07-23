﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voidwell.DaybreakGames.Models
{
    public class WorldState
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public bool IsOnline { get; private set; } = false;

        private ConcurrentDictionary<string, OnlineCharacter> OnlinePlayers { get; set; }
        private ConcurrentDictionary<int, WorldZoneState> ZoneStates { get; set; }
        private static readonly TimeSpan MaximumIdleDuration = TimeSpan.FromMinutes(10);

        public WorldState(int worldId, string worldName)
        {
            Id = worldId;
            Name = worldName;
            ZoneStates = new ConcurrentDictionary<int, WorldZoneState>();
            OnlinePlayers = new ConcurrentDictionary<string, OnlineCharacter>();
        }

        public void SetWorldOnline()
        {
            OnlinePlayers.Clear();
            ZoneStates.Clear();
            IsOnline = true;
        }

        public void SetWorldOffline()
        {
            ZoneStates.Clear();
            OnlinePlayers.Clear();
            IsOnline = false;
        }

        public void SetZoneState(WorldZoneState zoneState)
        {
            if (!ZoneStates.ContainsKey(zoneState.ZoneId))
            {
                ZoneStates.TryAdd(zoneState.ZoneId, zoneState);
                return;
            }

            ZoneStates[zoneState.ZoneId] = zoneState;
        }

        public IEnumerable<WorldOnlineZoneState> GetZoneStates()
        {
            return ZoneStates.Keys.Select(GetZoneState);
        }

        public WorldOnlineZoneState GetZoneState(int zoneId)
        {
            if (!ZoneStates.ContainsKey(zoneId))
            {
                return null;
            }

            var zoneState = ZoneStates[zoneId];
            return new WorldOnlineZoneState
            {
                Id = zoneId,
                Name = zoneState.Name,
                IsTracking = zoneState.IsTracking,
                LockState = zoneState.LockState,
                Population = GetZonePopulation(zoneId)
            };
        }
        
        public IEnumerable<ZoneRegionOwnership> GetZoneMapOwnership(int zoneId)
        {
            if (!ZoneStates.ContainsKey(zoneId))
            {
                return null;
            }

            return ZoneStates[zoneId].GetMapOwnership() ?? Enumerable.Empty<ZoneRegionOwnership>();
        }

        public MapScore GetZoneMapScore(int zoneId)
        {
            if (!ZoneStates.ContainsKey(zoneId))
            {
                return null;
            }

            return ZoneStates[zoneId].MapScore;
        }

        public void UpdateZoneLockState(int zoneId, ZoneLockState lockState = null)
        {
            if (!ZoneStates.ContainsKey(zoneId))
            {
                return;
            }

            ZoneStates[zoneId].UpdateLockState(lockState);
        }

        public async Task<FacilityControlChange> UpdateZoneFacilityFaction(int zoneId, int facilityId, int factionId)
        {
            if (!ZoneStates.ContainsKey(zoneId))
            {
                return null;
            }

            await ZoneStates[zoneId].FacilityFactionChange(facilityId, factionId);

            return new FacilityControlChange
            {
                Region = ZoneStates[zoneId].GetRegionByFacilityId(facilityId),
                Score = ZoneStates[zoneId].MapScore
            };
        }

        public void SetPlayerOnline(string characterId, string characterName, int factionId, DateTime timestamp)
        {
            var onlineCharacter = new OnlineCharacter
            {
                Character = new OnlineCharacterProfile
                {
                    CharacterId = characterId,
                    FactionId = factionId,
                    Name = characterName,
                    WorldId = Id
                },
                LoginDate = timestamp
            };

            OnlinePlayers.AddOrUpdate(characterId, onlineCharacter,
                (key, existing) =>
                {
                    existing.LoginDate = timestamp;
                    return existing;
                });
        }

        public OnlineCharacter SetPlayerOffline(string characterId)
        {
            OnlineCharacter onlineCharacter = null;
            OnlinePlayers.TryRemove(characterId, out onlineCharacter);
            return onlineCharacter;
        }

        public IEnumerable<OnlineCharacter> GetOnlinePlayers()
        {
            return OnlinePlayers.Values;
        }

        public int CountOnlinePlayers()
        {
            return OnlinePlayers.Values.Count;
        }

        public void SetPlayerLastSeen(string characterId, int zoneId, DateTime timestamp)
        {
            if (!OnlinePlayers.ContainsKey(characterId))
            {
                return;
            }

            OnlinePlayers[characterId].UpdateLastSeen(timestamp, zoneId);
        }

        public Dictionary<int, ZonePopulation> GetZonePopulations()
        {
            if (ZoneStates == null)
            {
                return null;
            }

            return ZoneStates.Keys.ToDictionary(a => a, a => GetZonePopulation(a));
        }

        public ZonePopulation GetZonePopulation(int zoneId)
        {
            if (OnlinePlayers == null)
            {
                return new ZonePopulation();
            }

            var zonePlayers = OnlinePlayers.Values.Where(p => p.Character.LastSeen != null && p.Character.LastSeen.ZoneId == zoneId && (DateTime.UtcNow - p.Character.LastSeen.Timestamp <= MaximumIdleDuration));
            var vsPlayers = zonePlayers.Count(p => p.Character.FactionId == 1);
            var ncPlayers = zonePlayers.Count(p => p.Character.FactionId == 2);
            var trPlayers = zonePlayers.Count(p => p.Character.FactionId == 3);

            return new ZonePopulation(vsPlayers, ncPlayers, trPlayers);
        }
    }
}
