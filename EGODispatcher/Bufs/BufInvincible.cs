namespace Bufs
{
    public class BufInvincible : UnitBuf
    {
        public BufInvincible()
        {
            duplicateType = BufDuplicateType.ONLY_ONE;
            type = UnitBufType.ADD_SUPERARMOR;
        }

        public override void Init(UnitModel model)
        {
            base.Init(model);
            SwitchInvincible(true);
        }

        public override void OnStageRelease()
        {
            SwitchInvincible(false);
        }

        public void SwitchInvincible(bool flag)
        {
            if (model is AgentModel agent)
            {
                agent.cannotBeAttackTargetable = flag;
                if (agent is WorkerModel worker)
                {
                    worker.SetInvincible(flag);
                }
            }
        }
    }
}
