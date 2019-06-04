﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voidwell.DaybreakGames.Models;
using Voidwell.DaybreakGames.Services.Planetside;

namespace Voidwell.DaybreakGames.Controllers.Planetside
{
    [Route("ps2/worldState")]
    public class WorldStateController : Controller
    {
        private readonly IWorldMonitor _worldMonitor;

        public WorldStateController(IWorldMonitor worldMonitor)
        {
            _worldMonitor = worldMonitor;
        }

        [HttpGet]
        public Task<IEnumerable<WorldOnlineState>> GetWorldStates()
        {
            return _worldMonitor.GetWorldStates();
        }

        [HttpGet("{worldId}")]
        public Task<WorldOnlineState> GetWorldState(int worldId)
        {
            return _worldMonitor.GetWorldState(worldId);
        }

        [HttpGet("{worldId}/players")]
        public Task<IEnumerable<OnlineCharacter>> GetOnlinePlayers(int worldId)
        {
            return _worldMonitor.GetOnlineCharactersByWorld(worldId);
        }

        [HttpGet("{worldId}/{zoneId}/map")]
        public IEnumerable<ZoneRegionOwnership> GetZoneOwnership(int worldId, int zoneId)
        {
            return _worldMonitor.GetZoneOwnership(worldId, zoneId);
        }

        [HttpPost("{worldId}/zone")]
        public Task SetupWorldZones(int worldId)
        {
            return _worldMonitor.SetupWorldZones(worldId);
        }
    }
}
