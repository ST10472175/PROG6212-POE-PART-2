using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{//start of namespace

    //data a participant sends to enter an event
    public class EnrolRequest
    {//start of class

        //the category (of that event) the participant is entering
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

    }//end of class

    //enrolment data sent back to the user
    public class EnrolmentResponse
    {//start of class

        public int EntryId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime EntryDate { get; set; }

    }//end of class

}//end of namespace
