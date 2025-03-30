﻿using Core.DataAccess.EntityFramework.Repository;
using Core.Utilities.Results;
using DataAccess.EntityFramework;
using DataAccess.Interfaces;
using Entities.Concrete;
using Entities.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
  public  class AppUserRepository:Repository<AppUser, TexSoftContext>,IAppUserDal
    {
        //public AppUserRepository(TexSoftContext context) : base(context)
        //{

        //}

       public async Task<List<UserAndRoleDto>> GetUsersAndRoles()
        {
            using (TexSoftContext _context = new TexSoftContext())
            {
                var result =  (from a in _context.AppUsers
                                    join b in _context.AppRole on a.RoleId equals b.Id
                                    where a.RoleId==b.Id 
                                    select new UserAndRoleDto 
                                    { RoleId=b.Id,RoleName=b.RoleName,UserId=a.Id,UserName=a.UserName}).ToList();
                return result;
            }
            return null;
        }
        public async Task<UserAndRoleDto> GetUserAndRole(long userId)
        {
            using (TexSoftContext _context = new TexSoftContext())
            {
                var result =  (from a in _context.AppUsers
                                    join b in _context.AppRole on a.RoleId equals b.Id
                                    where a.RoleId == b.Id && a.Id == userId
                                    select new UserAndRoleDto { RoleId = b.Id, RoleName = b.RoleName, UserId = a.Id, UserName = a.UserName,Email=a.Email }
                                   ).FirstOrDefault();
                return result;
            }
     
        }
    }
}
