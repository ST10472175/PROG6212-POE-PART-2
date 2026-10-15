namespace RaceDay.Api.Models
{//start of namespace
    public class Assignment
    {//start of class

        //this is the enrolment table (called Assignment in the Part 1 ERD)
        //database columns are entryId, eventId, participantId, categoryId and entryDate
        public int EntryId { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public int ParticipantId { get; set; }
        public Participant Participant { get; set; }

        //added because the brief says the enrolment must record the chosen category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public DateTime EntryDate { get; set; }

    }//end of class
}//end of namespace
