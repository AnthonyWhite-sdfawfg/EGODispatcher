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
            if (model is WorkerModel worker)
            {
                worker.SetInvincible(true);
            }
        }

        public override void OnStageRelease()
        {
            RemoveInvincible();
        }

        public void RemoveInvincible()
        {
            if (model is WorkerModel)
            {
                (model as WorkerModel).SetInvincible(false);
            }
        }
    }
}
