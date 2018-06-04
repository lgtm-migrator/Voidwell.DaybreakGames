﻿using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voidwell.DaybreakGames.Data.Models.Planetside;

namespace Voidwell.DaybreakGames.Data.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly IDbContextHelper _dbContextHelper;

        public VehicleRepository(IDbContextHelper dbContextHelper)
        {
            _dbContextHelper = dbContextHelper;
        }

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                return await dbContext.Vehicles.Include(i => i.Faction)
                    .ToListAsync();
            }
        }

        public async Task UpsertRangeAsync(IEnumerable<Vehicle> entities)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var dbSet = dbContext.Vehicles;

                foreach (var entity in entities)
                {
                    var storeEntity = await dbSet.SingleOrDefaultAsync(a => a.Id == entity.Id);
                    if (storeEntity == null)
                    {
                        dbSet.Add(entity);
                    }
                    else
                    {
                        storeEntity = entity;
                        dbSet.Update(storeEntity);
                    }
                }

                await dbContext.SaveChangesAsync();
            }
        }

        public async Task UpsertRangeAsync(IEnumerable<VehicleFaction> entities)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var dbSet = dbContext.VehicleFactions;

                foreach (var entity in entities)
                {
                    var storeEntity = await dbSet.SingleOrDefaultAsync(a => a.VehicleId == entity.VehicleId && a.FactionId == entity.FactionId);
                    if (storeEntity == null)
                    {
                        dbSet.Add(entity);
                    }
                    else
                    {
                        storeEntity = entity;
                        dbSet.Update(storeEntity);
                    }
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
