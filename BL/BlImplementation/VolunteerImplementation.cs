using BlApi;
using DalApi;

namespace BlImplementation
{
    internal class VolunteerImplementation : BlApi.IVolunteer
    {
        private readonly IDal _dal = DalApi.Factory.Get;

        public BO.Role Login(string name, string password)
        {
            try
            {
                var volunteer = _dal.Volunteer.GetAll()
                    .FirstOrDefault(v => v.Name == name && v.Password == password)
                    ?? throw new BO.BlMissingEntityException("Volunteer not found or invalid credentials");

                return volunteer.Role;
            }
            catch (DO.DalException ex)
            {
                throw new BO.BlException("Login failed", ex);
            }
        }

        public IEnumerable<BO.VolunteerInList> GetVolunteersList(bool? isActive, BO.TypeOfCall? callType)
        {
            try
            {
                var volunteers = _dal.Volunteer.ReadAll();

                // Filter by active status if specified
                if (isActive.HasValue)
                    volunteers = volunteers.Where(v => v.IsActive == isActive.Value);

                // Convert to BO.VolunteerInList with LINQ
                var result = from v in volunteers
                             let currentCall = _dal.Call.ReadAll()
                                 .FirstOrDefault(c => c.AssignmentList != null &&
                                     c..Any(a => a.VolunteerId == v.ID && !a.FinishingTime.HasValue))
                             select new BO.VolunteerInList
                             {
                                 Id = v.Id,
                                 Name = v.Name,
                                 IsActive = v.IsActive,
                                 TreatedCallNum = v.TreatedCallNum,
                                 CenteledCallNum = v.CenteledCallNum,
                                 OutOfRangeCallNum = v.OutOfRangeCallNum,
                                 CallId = currentCall?.Id ?? 0,
                                 TypeOfCall = currentCall?.TypeOfCall ?? default
                             };

                // Filter by call type if specified
                if (callType.HasValue)
                    result = result.Where(v => v.TypeOfCall == callType.Value);

                return result;
            }
            catch (DO.DalException ex)
            {
                throw new BO.BlException("Failed to retrieve volunteers list", ex);
            }
        }

        public BO.Volunteer GetVolunteerDetails(int volunteerId)
        {
            try
            {
                var volunteer = _dal.Volunteer.Get(volunteerId);
                var currentCall = _dal.Call.GetAll()
                    .FirstOrDefault(c => c.AssignmentList != null &&
                        c.AssignmentList.Any(a => a.VolunteerId == volunteerId && !a.FinishingTime.HasValue));

                return new BO.Volunteer
                {
                    Id = volunteer.Id,
                    Name = volunteer.Name,
                    Phone = volunteer.Phone,
                    Email = volunteer.Email,
                    Password = volunteer.Password,
                    Address = volunteer.Address,
                    Latitude = volunteer.Latitude,
                    Longitude = volunteer.Longitude,
                    Role = volunteer.Role,
                    IsActive = volunteer.IsActive,
                    MaxDistanceForCall = volunteer.MaxDistanceForCall,
                    DistanceType = volunteer.DistanceType,
                    TreatedCallNum = volunteer.TreatedCallNum,
                    CenteledCallNum = volunteer.CenteledCallNum,
                    OutOfRangeCallNum = volunteer.OutOfRangeCallNum,
                    CallInProgress = currentCall != null ? new BO.CallInProgress
                    {
                        Id = currentCall.Id,
                        CallId = currentCall.Id,
                        TypeOfCall = currentCall.TypeOfCall,
                        CallDescription = currentCall.CallDescription,
                        Address = currentCall.Address,
                        OpeningTime = currentCall.OpeningTime,
                        MaxTimeForClosing = currentCall.MaxTimeForClosing,
                        EntraceTime = currentCall.AssignmentList.First(a => a.VolunteerId == volunteerId).EntranceTime,
                        Distance = CalculateDistance(volunteer.Latitude ?? 0, volunteer.Longitude ?? 0,
                            currentCall.Latitude, currentCall.Longitude),
                        CallTritingByVulanteerStatus = BO.CallTritingByVulanteerStatus.Treating
                    } : null
                };
            }
            catch (DO.DalException ex)
            {
                throw new BO.BlException($"Failed to retrieve volunteer details for ID: {volunteerId}", ex);
            }
        }

        public void UpdateVolunteerDetails(int requesterId, BO.Volunteer volunteer)
        {
            try
            {
                // Verify requester has permission
                var requester = _dal.Volunteer.Get(requesterId);
                if (requester.Role != BO.Role.Manager && requesterId != volunteer.Id)
                    throw new BO.BlSecurityException("Unauthorized to update volunteer details");

                _dal.Volunteer.Update(new DO.Volunteer
                {
                    Id = volunteer.Id,
                    Name = volunteer.Name,
                    Phone = volunteer.Phone,
                    Email = volunteer.Email,
                    Password = volunteer.Password,
                    Address = volunteer.Address,
                    Latitude = volunteer.Latitude,
                    Longitude = volunteer.Longitude,
                    Role = volunteer.Role,
                    IsActive = volunteer.IsActive,
                    MaxDistanceForCall = volunteer.MaxDistanceForCall,
                    DistanceType = volunteer.DistanceType,
                    TreatedCallNum = volunteer.TreatedCallNum,
                    CenteledCallNum = volunteer.CenteledCallNum,
                    OutOfRangeCallNum = volunteer.OutOfRangeCallNum
                });
            }
            catch (DO.DalException ex)
            {
                throw new BO.BlException($"Failed to update volunteer details for ID: {volunteer.Id}", ex);
            }
        }

        public void DeleteVolunteer(int volunteerId)
        {
            try
            {
                _dal.Volunteer.Delete(volunteerId);
            }
            catch (DO.DalException ex)
            {
                throw new BO.BlException($"Failed to delete volunteer with ID: {volunteerId}", ex);
            }
        }

        public void AddVolunteer(BO.Volunteer volunteer)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(volunteer.Name))
                    throw new BO.BlInvalidInputException("Volunteer name is required");
                if (string.IsNullOrEmpty(volunteer.Phone))
                    throw new BO.BlInvalidInputException("Volunteer phone is required");
                if (string.IsNullOrEmpty(volunteer.Email))
                    throw new BO.BlInvalidInputException("Volunteer email is required");

                _dal.Volunteer.Add(new DO.Volunteer
                {
                    Name = volunteer.Name,
                    Phone = volunteer.Phone,
                    Email = volunteer.Email,
                    Password = volunteer.Password,
                    Address = volunteer.Address,
                    Latitude = volunteer.Latitude,
                    Longitude = volunteer.Longitude,
                    Role = volunteer.Role,
                    IsActive = volunteer.IsActive,
                    MaxDistanceForCall = volunteer.MaxDistanceForCall,
                    DistanceType = volunteer.DistanceType,
                    TreatedCallNum = 0,
                    CenteledCallNum = 0,
                    OutOfRangeCallNum = 0
                });
            }
            catch (DO.DalException ex)
            {
                throw new BO.BlException("Failed to add volunteer", ex);
            }
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRad(double degrees) => degrees * Math.PI / 180;
    }
}