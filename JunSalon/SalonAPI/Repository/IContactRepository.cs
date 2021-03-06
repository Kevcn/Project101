﻿using System.Threading.Tasks;
using SalonAPI.Domain;

namespace SalonAPI.Repository
{
    public interface IContactRepository
    {
        Task<int> GetContactID(Contact contact);
        Task<int> AddContact(Contact contact);
        Task<Contact> CheckDuplicate(Contact contact);
    }
}