using BlApi;
using Helpers;

namespace BlImplementation
{
    internal class CallImplementation : ICall
    {
        private readonly DalApi.IDal Call_dal = DalApi.Factory.Get;

        private void ValidateCall(BO.Call boCall)
        {
            try
            {
                if (boCall == null)
                    throw new BO.BlNullPropertyException("The call object cannot be null.");

                if (boCall.Id <= 0)
                    throw new BO.BlInvalidArgumentException("Call ID must be a positive number.");

                if (boCall.MaxTimeForClosing.HasValue && boCall.MaxTimeForClosing.Value <= boCall.OpeningTime)
                    throw new BO.BlInvalidArgumentException("End time must be greater than the start time.");

                if (string.IsNullOrWhiteSpace(boCall.Address))
                    throw new BO.BlInvalidArgumentException("Address cannot be null or empty.");

                (double lat, double lon) = CallManager.GetCoordinates(boCall.Address);

                if (lat == 0 || lon == 0)
                    throw new BO.BlInvalidArgumentException("Address is invalid or could not be found.");

                boCall.Latitude = lat;
                boCall.Longitude = lon;

                if (boCall.CallStatus == BO.CallStatus.closed && boCall.OpeningTime > DateTime.Now)
                    throw new BO.BlInvalidOperationException("A closed call cannot have a start time in the future.");
            }
            catch (BO.BlNullPropertyException ex)
            {
                throw new BO.BlNullPropertyException("Error during call validation: " + ex.Message, ex);
            }
            catch (BO.BlInvalidArgumentException ex)
            {
                throw new BO.BlInvalidArgumentException("Error during call validation: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new BO.BlGeneralException("An unexpected error occurred while validating the call.", ex);
            }
        }

        public void ChoosingACallForTreatment(int volunteerId, int callId)
        {
            try
            {
                var call = Call_dal.Call.Read(callId)
                    ?? throw new BO.BlDoesNotExistException("The call does not exist in the system.");

                if (call.Status != DO.CallStatus.Open && call.Status != DO.CallStatus.OpenInRisk)
                    throw new BO.BlInvalidOperationException("This call is not open for treatment.");

                if (Call_dal.Assignment.ReadAll().Any(a => a.CallId == callId))
                    throw new BO.BlAlreadyExistsException("This call is already being handled.");

                var newAssignment = new DO.Assignment
                {
                    VolunteerId = volunteerId,
                    CallId = callId,
                    EntryTimeForTreatment = ClockManager.Now,
                    EndTimeForTreatment = null,
                };

                Call_dal.Assignment.Create(newAssignment);
            }
            catch (BO.BlDoesNotExistException ex)
            {
                throw new BO.BlDoesNotExistException("Error while choosing call for treatment: " + ex.Message, ex);
            }
            catch (BO.BlInvalidOperationException ex)
            {
                throw new BO.BlInvalidOperationException("Error while choosing call for treatment: " + ex.Message, ex);
            }
            catch (BO.BlAlreadyExistsException ex)
            {
                throw new BO.BlAlreadyExistsException("Error while choosing call for treatment: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new BO.BlGeneralException("An unexpected error occurred while choosing a call for treatment.", ex);
            }
        }

        public void Create(BO.Call boCall)
        {
            try
            {
                ValidateCall(boCall);

                var doCall = new DO.Call
                {
                    ID = boCall.Id,
                    CallDescription = boCall.CallDescription,
                    Address = boCall.Address,
                    Latitude = boCall.Latitude,
                    Longitude = boCall.Longitude,
                    OpeningTime = boCall.OpeningTime,
                    MaxTimeForClosing = boCall.MaxTimeForClosing
                };

                try
                {
                    Call_dal.Call.Create(doCall);
                }
                catch (DO.DalAlreadyExistsException ex)
                {
                    throw new BO.BlAlreadyExistsException($"Call with ID={boCall.Id} already exists.", ex);
                }
            }
            catch (BO.BlNullPropertyException ex)
            {
                throw new BO.BlNullPropertyException("Error during create operation: " + ex.Message, ex);
            }
            catch (BO.BlInvalidArgumentException ex)
            {
                throw new BO.BlInvalidArgumentException("Error during create operation: " + ex.Message, ex);
            }
            catch (BO.BlAlreadyExistsException ex)
            {
                throw new BO.BlAlreadyExistsException("Error during create operation: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new BO.BlGeneralException("An unexpected error occurred during the create operation.", ex);
            }
        }

        public void Delete(int id)
        {
            try
            {
                var call = Call_dal.Call.Read(id)
                    ?? throw new BO.BlDoesNotExistException("The call does not exist in the system.");

                if (call.Status != DO.CallStatus.Open || Call_dal.Assignment.ReadAll().Any(a => a.CallId == id))
                    throw new BO.BlInvalidOperationException("This call cannot be deleted.");

                try
                {
                    Call_dal.Call.Delete(id);
                }
                catch (DO.DalDoesNotExistException ex)
                {
                    throw new BO.BlInternalErrorException("An error occurred while deleting the call.", ex);
                }
            }
            catch (BO.BlDoesNotExistException ex)
            {
                throw new BO.BlDoesNotExistException("Error while deleting call: " + ex.Message, ex);
            }
            catch (BO.BlInvalidOperationException ex)
            {
                throw new BO.BlInvalidOperationException("Error while deleting call: " + ex.Message, ex);
            }
            catch (BO.BlInternalErrorException ex)
            {
                throw new BO.BlInternalErrorException("Error while deleting call: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new BO.BlGeneralException("An unexpected error occurred while deleting the call.", ex);
            }
        }

        public BO.Call GetCallDetails(int id)
        {
            try
            {
                var doCall = Call_dal.Call.Read(id)
                    ?? throw new BO.BlDoesNotExistException("The call does not exist in the system.");

                var assignments = Call_dal.Assignment.ReadAll()
                    .Where(a => a.CallId == id)
                    .Select(a => new BO.CallAssignInList
                    {
                        VolunteerId = a.VolunteerId,
                        EntranceTime = a.EntryTimeForTreatment,
                        FinishingTime = a.EndTimeForTreatment,
                    }).ToList();

                return new BO.Call
                {
                    Id = doCall.ID,
                    CallDescription = doCall.CallDescription,
                    Status = (BO.CallStatus)doCall.Status,
                    Address = doCall.Address,
                    Latitude = doCall.Latitude,
                    Longitude = doCall.Longitude,
                    OpeningTime = doCall.OpeningTime,
                    MaxTimeForClosing = doCall.MaxTimeForClosing,
                    AssignedVolunteers = assignments
                };
            }
            catch (BO.BlDoesNotExistException ex)
            {
                throw new BO.BlDoesNotExistException("Error while getting call details: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new BO.BlGeneralException("An unexpected error occurred while getting call details.", ex);
            }
        }

        public IEnumerable<int> GetCallsCount()
        {
            try
            {
                return Call_dal.Call.ReadAll()
                    .GroupBy(c => c.Status)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Count())
                    .ToArray();
            }
            catch (Exception ex)
            {
                throw new BO.BlGeneralException("An error occurred while getting the call counts.", ex);
            }
        }
    }
}
