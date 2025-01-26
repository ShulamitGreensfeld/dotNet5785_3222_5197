using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BlApi;
using BO;
using DalApi;

namespace BlImplementation
{
    internal class VolunteerImplementation : IVolunteer
    {
        private readonly IDal _dal = Factory.Get;

        // Login method
        public Role Login(string name, string password)
        {
            try
            {
                var volunteer = _dal.Volunteer.GetByName(name);
                if (volunteer == null || volunteer.Password != password)
                    throw new BlInvalidLoginException("Invalid username or password");

                return volunteer.Role;
            }
            catch (DO.DalEntityNotFoundException ex)
            {
                throw new BlEntityNotFoundException("Volunteer not found", ex);
            }
        }

        // Get Volunteers List method
        public IEnumerable<Volunteer> GetVolunteersList(bool? isActive, TypeOfCall? callType)
        {
            try
            {
                // Get all volunteers from DAL
                var volunteers = _dal.Volunteer.ReadAll();

                // Filter by active status if specified
                if (isActive.HasValue)
                    volunteers = volunteers.Where(v => v.IsActive == isActive.Value);

                // Filter by call type if specified
                if (callType.HasValue)
                    volunteers = volunteers.Where(v =>
                        v.CallInProgress != null &&
                        v.CallInProgress.TypeOfCall == callType.Value);

                // Convert to Business Logic Volunteers
                return volunteers.Select(v => new Volunteer
                {
                    // Map DAL Volunteer to BO Volunteer
                    Id = v.ID,
                    Name = v.Name,
                    Phone = v.Phone,
                    Email = v.Email,
                    IsActive = v.IsActive,
                    Role = v.Role,
                });
            }
            catch (Exception ex)
            {
                throw new BlDataAccessException("Error retrieving volunteers", ex);
            }
        }

        // Get Volunteer Details method
        public Volunteer GetVolunteerDetails(int volunteerId)
        {
            try
            {
                var dalVolunteer = _dal.Volunteer.Get(volunteerId);
                var dalCall = _dal.Call.GetVolunteerCurrentCall(volunteerId);

                return new Volunteer
                {
                    Id = dalVolunteer.Id,
                    Name = dalVolunteer.Name,
                    Phone = dalVolunteer.Phone,
                    Email = dalVolunteer.Email,
                    Address = dalVolunteer.Address,
                    Latitude = dalVolunteer.Latitude,
                    Longitude = dalVolunteer.Longitude,
                    Role = dalVolunteer.Role,
                    IsActive = dalVolunteer.IsActive,
                    CallInProgress = dalCall != null ? new CallInProgress
                    {
                        Id = dalCall.Id,
                        TypeOfCall = dalCall.CallType
                    } : null
                };
            }
            catch (DO.DalEntityNotFoundException ex)
            {
                throw new BlEntityNotFoundException("Volunteer not found", ex);
            }
        }

        // Update Volunteer Details method
        public void UpdateVolunteerDetails(int requesterId, Volunteer volunteer)
        {
            try
            {
                // Validate requester
                var requester = _dal.Volunteer.Read(requesterId);
                var existingVolunteer = _dal.Volunteer.Read(volunteer.Id);

                // Check if requester is admin or the volunteer themselves
                if (requester.Role != Role.Manager && requester.ID != volunteer.Id)
                    throw new BlUnauthorizedAccessException("Not authorized to update volunteer details");

                // Validate email format
                if (!IsValidEmail(volunteer.Email))
                    throw new BlInvalidInputException("Invalid email format");

                // Validate ID format (assuming Israeli ID validation)
                if (!IsValidIsraeliId(volunteer.Id.ToString()))
                    throw new BlInvalidInputException("Invalid ID number");

                // Check which fields can be updated based on requester
                var updatableFields = GetUpdatableFields(requester, existingVolunteer, volunteer);

                // Update DAL volunteer
                _dal.Volunteer.Update(new DO.Volunteer
                {
                    ID = volunteer.Id,
                    Name = updatableFields.Contains(nameof(volunteer.Name)) ? volunteer.Name : existingVolunteer.Name,
                    Phone = updatableFields.Contains(nameof(volunteer.Phone)) ? volunteer.Phone : existingVolunteer.Phone,
                    Email = updatableFields.Contains(nameof(volunteer.Email)) ? volunteer.Email : existingVolunteer.Email,
                    Address = updatableFields.Contains(nameof(volunteer.Address)) ? volunteer.Address : existingVolunteer.Address,
                    Role = updatableFields.Contains(nameof(volunteer.Role)) ? volunteer.Role : existingVolunteer.Role,
                    IsActive = updatableFields.Contains(nameof(volunteer.IsActive)) ? volunteer.IsActive : existingVolunteer.IsActive
                });
            }
            catch (DO.DalEntityNotFoundException ex)
            {
                throw new BlEntityNotFoundException("Volunteer not found", ex);
            }
        }

        // Delete Volunteer method
        public void DeleteVolunteer(int volunteerId)
        {
            try
            {
                // Check if volunteer has any active or past calls
                var hasActiveCalls = _dal.Call.GetVolunteerCalls(volunteerId).Any();
                if (hasActiveCalls)
                    throw new BlInvalidOperationException("Cannot delete volunteer with call history");

                // Attempt to delete
                _dal.Volunteer.Delete(volunteerId);
            }
            catch (DO.DalEntityNotFoundException ex)
            {
                throw new BlEntityNotFoundException("Volunteer not found", ex);
            }
        }

        // Add Volunteer method
        public void AddVolunteer(Volunteer volunteer)
        {
            try
            {
                // Validate input
                if (!IsValidEmail(volunteer.Email))
                    throw new BlInvalidInputException("Invalid email format");

                if (!IsValidIsraeliId(volunteer.Id.ToString()))
                    throw new BlInvalidInputException("Invalid ID number");

                // Check if volunteer already exists
                try
                {
                    _dal.Volunteer.Read(volunteer.Id);
                    throw new BlDuplicateEntityException("Volunteer with this ID already exists");
                }
                catch (DO.DalEntityNotFoundException)
                {
                    // This is good - volunteer doesn't exist
                }

                // Convert to DAL Volunteer
                _dal.Volunteer.Create(new DO.Volunteer
                {
                    ID = volunteer.Id,
                    Name = volunteer.Name,
                    Phone = volunteer.Phone,
                    Email = volunteer.Email,
                    Address = volunteer.Address,
                    Role = volunteer.Role,
                    IsActive = volunteer.IsActive
                });
            }
            catch (DO.DalEntityAlreadyExistsException ex)
            {
                throw new BlDuplicateEntityException("Volunteer already exists", ex);
            }
        }

        // Helper Methods
        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsValidIsraeliId(string id)
        {
            // Basic Israeli ID validation with check digit
            if (id.Length != 9 || !id.All(char.IsDigit))
                return false;

            int sum = 0;
            for (int i = 0; i < 8; i++)
            {
                int digit = int.Parse(id[i].ToString());
                sum += (i % 2 == 0) ? digit : ((digit * 2 > 9) ? digit * 2 - 9 : digit * 2);
            }

            int checkDigit = int.Parse(id[8].ToString());
            return (sum + checkDigit) % 10 == 0;
        }

        private List<string> GetUpdatableFields(DO.Volunteer requester, DO.Volunteer existingVolunteer, Volunteer newVolunteer)
        {
            var updatableFields = new List<string>();

            // Fields always updatable
            if (newVolunteer.Name != existingVolunteer.Name)
                updatableFields.Add(nameof(newVolunteer.Name));
            if (newVolunteer.Phone != existingVolunteer.Phone)
                updatableFields.Add(nameof(newVolunteer.Phone));
            if (newVolunteer.Email != existingVolunteer.Email)
                updatableFields.Add(nameof(newVolunteer.Email));
            if (newVolunteer.Address != existingVolunteer.Address)
                updatableFields.Add(nameof(newVolunteer.Address));

            // Admin-only fields
            if (requester.Role == Role.Admin)
            {
                if (newVolunteer.Role != existingVolunteer.Role)
                    updatableFields.Add(nameof(newVolunteer.Role));
                if (newVolunteer.IsActive != existingVolunteer.IsActive)
                    updatableFields.Add(nameof(newVolunteer.IsActive));
            }

            return updatableFields;
        }
    }

}