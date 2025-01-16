

namespace Main.Models
{
    public class Limit
    {
        public int LimitId {  get; set; }
        public int FamilyId {  get; set; }
        public int UserId {  get; set; }
        public int CategoryId {  get; set; }
        public double LimitAmount {  get; set; }
        public int FrequencyId {  get; set; }
        public int IsFamilyWide {  get; set; }
        public int CreatedByUserID { get; set; }

        public Limit()
        {
        }
        public Limit(int limitId, int familyId, int userId, int categoryId, double limitAmount, int frequencyId, int isFamilyWide, int createdByUserID)
        {
            LimitId = limitId;
            FamilyId = familyId;
            UserId = userId;
            CategoryId = categoryId;
            LimitAmount = limitAmount;
            FrequencyId = frequencyId;
            IsFamilyWide = isFamilyWide;
            CreatedByUserID = createdByUserID;
        }
    }
}
