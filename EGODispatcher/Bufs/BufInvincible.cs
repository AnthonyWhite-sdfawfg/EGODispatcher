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
            SwitchStatus(true);
        }

        public override void OnStageRelease()
        {
            SwitchStatus(false);
        }

        public void SwitchStatus(bool flag)
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
