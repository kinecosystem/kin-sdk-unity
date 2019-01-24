namespace Kin
{
	[System.Serializable]
	public class PaymentInfo
	{
		public string CreatedAt;
		public string DestinationPublicKey;
		public string SourcePublicKey;
		public string _Amount;
		public decimal Amount { get { return decimal.Parse( _Amount, System.Globalization.NumberStyles.Float ); } }
		public string Hash;
		public string Memo;
		public string AccountId;


		public override string ToString()
		{
			return string.Format( "CreatedAt: {0}, DestinationPublicKey: {1}, SourcePublicKey: {2}, Amount: {3}, Hash: {4}, Memo: {5}",
			                     CreatedAt, DestinationPublicKey, SourcePublicKey, Amount, Hash, Memo );
		}
	}
}