﻿using System.Threading.Tasks;
using Voidwell.DaybreakGames.Data.Models.Planetside;

namespace Voidwell.DaybreakGames.Data.Repositories
{
    public interface IWeaponAggregateRepository
    {
        Task<WeaponAggregate> GetWeaponAggregateByItemId(int itemId);
        Task<WeaponAggregate> GetWeaponAggregateByVehicleId(int vehicleId);
    }
}
