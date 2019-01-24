namespace Kin
{
	/// <summary>
	/// interface that can be implemented for listening to ongoing payments on a KinAccount
	/// </summary>
	public interface IPaymentListener
	{
		void OnEvent( PaymentInfo data );
	}


	/// <summary>
	/// interface that can be implemented for listening to balance changes on a KinAccount
	/// </summary>
	public interface IBalanceListener
	{
		void OnEvent( decimal data );
	}


	/// <summary>
	/// interface that can be implemented for listening to account creation on a KinAccount
	/// </summary>
	public interface IAccountCreationListener
	{
		void OnEvent();
	}
}