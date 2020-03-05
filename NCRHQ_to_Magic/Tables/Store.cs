namespace NCRHQ_to_Magic.Tables
{
    class Store
    {
        public Store()
        {

        }
        public Store(int STORE_ID, string PRM_STORE_NUMBER)
        {
            this.STORE_ID = STORE_ID;
            this.PRM_STORE_NUMBER = PRM_STORE_NUMBER;
        }
        public int STORE_ID { get; set; }
        public string PRM_STORE_NUMBER { get; set; }
    }
}
