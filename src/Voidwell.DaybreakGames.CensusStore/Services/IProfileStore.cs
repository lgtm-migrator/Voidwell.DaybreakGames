﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Voidwell.DaybreakGames.Data.Models.Planetside;

namespace Voidwell.DaybreakGames.CensusStore.Services
{
    public interface IProfileStore : IUpdateable
    {
        Task<IEnumerable<Profile>> GetAllProfilesAsync();
        Task<IEnumerable<Loadout>> GetAllLoadoutsAsync();
    }
}