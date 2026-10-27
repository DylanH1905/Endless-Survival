namespace ESS.Objects.Player.Archer.Skills
{
	public interface IChargeableSkill
	{
		bool IsCharging { get; }
		float ChargeProgress { get; }
		void BeginCharge();
		void ReleaseCharge();
	}
}
