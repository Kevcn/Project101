﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using SalonAPI.Configuration;
using SalonAPI.Domain;

namespace SalonAPI.Repository
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly MySqlConfig _mySqlConfig;

        public AppointmentRepository(IOptions<MySqlConfig> mySqlConfig)
        {
            _mySqlConfig = mySqlConfig.Value;
        }

        public async Task<List<BookingRecord>> GetAppointmentsByDay(DateTime startDate, DateTime endDate)
        {
            const string selectBookingRecords = @"
            SELECT 
                TimeSlotID, 
                Date 
            FROM bookingrecord 
            WHERE Date > @StartDate 
                AND Date < @EndDate
                AND Cancel = 0";

            try
            {
                await using var _connection = new MySqlConnection(_mySqlConfig.ConnectionString);
                var bookingRecords = await _connection.QueryAsync<BookingRecord>(selectBookingRecords, new
                {
                    StartDate = startDate,
                    EndDate = endDate
                });

                return bookingRecords.ToList();
            }
            catch (MySqlException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
            catch (InvalidOperationException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
        }

        public async Task<List<BookingRecord>> GetSingleDayAppointments(DateTime date)
        {
            const string selectBookingRecords = @"
            SELECT 
                TimeSlotID, 
                Date 
            FROM bookingrecord 
            WHERE Date = @Date
                AND Cancel = 0";

            try
            {
                await using var _connection = new MySqlConnection(_mySqlConfig.ConnectionString);
                var bookingRecords = await _connection.QueryAsync<BookingRecord>(selectBookingRecords, new
                {
                    Date = date
                });

                return bookingRecords.ToList();
            }
            catch (MySqlException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
            catch (InvalidOperationException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
        }

        public async Task<bool> AddAppointment(BookingRecord bookingRecord, int contactID)
        {
            const string addAppointment = @"
            INSERT INTO bookingrecord (
                ContactID, 
                TimeSlotID, 
                Date, 
                Description,
                CreatedDate,
                Cancel) 
            VALUES (
                @ContactID, 
                @TimeSlotID, 
                @Date, 
                @Description, 
                @CreatedDate,
                @Cancel);";

            try
            {
                await using var _connection = new MySqlConnection(_mySqlConfig.ConnectionString);
                var inserted = await _connection.ExecuteAsync(addAppointment,
                    new
                    {
                        ContactID = contactID,
                        TimeslotID = bookingRecord.TimeSlotID,
                        bookingRecord.Date,
                        bookingRecord.Description,
                        CreatedDate = DateTime.Now,
                        Cancel = false
                    });

                return inserted > 0;
            }
            catch (MySqlException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
            catch (InvalidOperationException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
        }

        public async Task<bool> VerifyTimeSlotAvailable(BookingRecord bookingRecord)
        {
            const string verifyTimeSlot = @"
            SELECT 
                Count(*)
            FROM bookingrecord 
            WHERE Date = @Date
                AND TimeSlotID = @TimeSlotID
                AND Cancel = 0";

            try
            {
                await using var _connection = new MySqlConnection(_mySqlConfig.ConnectionString);
                var bookingRecords = await _connection.QueryAsync<int>(verifyTimeSlot, new
                {
                    bookingRecord.Date, bookingRecord.TimeSlotID
                });

                return bookingRecords.Single() == 0;
            }
            catch (MySqlException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
            catch (InvalidOperationException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
        }

        public async Task<bool> CancelAppointment(int bookingID)
        {
            const string cancelAppointment = @"
            UPDATE bookingrecord
                SET Cancel = true
            WHERE ID = @ID";

            try
            {
                await using var _connection = new MySqlConnection(_mySqlConfig.ConnectionString);
                var cancelled = await _connection.ExecuteAsync(cancelAppointment,
                    new
                    {
                        ID = bookingID
                    });

                return cancelled == 1;
            }
            catch (MySqlException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
            catch (InvalidOperationException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
        }

        public async Task<List<BookingRecord>> GetAppointmentsByContactID(int contactID)
        {
            const string selectBookingRecords = @"
            SELECT
                ID,
                TimeSlotID,
                Date,
                Description
            FROM bookingrecord
            WHERE ContactID = @ContactID
                AND Date > NOW()
                AND Cancel = 0";

            try
            {
                await using var _connection = new MySqlConnection(_mySqlConfig.ConnectionString);
                var bookingRecords = await _connection.QueryAsync<BookingRecord>(selectBookingRecords, new
                {
                    ContactID = contactID
                });

                return bookingRecords.ToList();
            }
            catch (MySqlException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
            catch (InvalidOperationException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
        }

        public async Task<List<BookingRecord>> GetAppointmentsByDate(DateTime startDate, DateTime endDate)
        {
            const string selectBookingRecords = @"
                        SELECT 
                            b.ID,
                            TimeSlotID,
                            Date,
                            Description,
                            Cancel,
                            Name,                
                            Phone,
                            Email
                        FROM bookingrecord b
                        JOIN contact c
                        ON b.ContactID = c.ID
                        WHERE Date >= @StartDate
                            AND	Date <= @EndDate
                            AND Cancel = 0
                        ORDER BY Date ";

            try
            {
                await using var _connection = new MySqlConnection(_mySqlConfig.ConnectionString);
                var bookingRecords = await _connection.QueryAsync<BookingRecord, Contact, BookingRecord>(
                    selectBookingRecords,
                    (bookingRecord, contact) =>
                    {
                        bookingRecord.contact = contact;
                        return bookingRecord;
                    },
                    splitOn: "Name",
                    param: new
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    }
                );

                return bookingRecords.ToList();
            }
            catch (MySqlException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
            catch (InvalidOperationException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
        }

        public async Task<BookingRecord> GetAppointmentByID(int bookingID)
        {
            const string selectBookingRecord = @"
            SELECT 
	            b.ID,	
	            TimeSlotID,
	            Date,
	            Description,
                Cancel,
                Name,
	            Phone,
	            Email
            FROM bookingrecord b
            JOIN contact c
            ON b.ContactID = c.ID
            WHERE b.ID = @BookingID";

            try
            {
                await using var _connection = new MySqlConnection(_mySqlConfig.ConnectionString);
                var bookingRecord = await _connection.QueryAsync<BookingRecord, Contact, BookingRecord>(
                    selectBookingRecord,
                    (record, contact) =>
                    {
                        record.contact = contact;
                        return record;
                    },
                    splitOn: "Name",
                    param: new
                    {
                        BookingID = bookingID
                    }
                );

                return bookingRecord.Any() ? bookingRecord.Single() : new BookingRecord();
            }
            catch (MySqlException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
            catch (InvalidOperationException exception)
            {
                // TODO: log expection
                Console.WriteLine(exception);
                throw;
            }
        }
    }
}