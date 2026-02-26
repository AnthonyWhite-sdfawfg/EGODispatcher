using System;
using Spine.Unity;

namespace Creature
{
    public class EGODispatcherAnim : CreatureAnimScript
    {
        public void SetScript(EGODispatcher script)
        {
            this.script = script;
            this.animator = base.gameObject.GetComponent<SkeletonAnimation>();
            this.animator.AnimationState.SetAnimation(0, "default", true);
        }
        public void work()
        {
            this.animator.AnimationState.SetAnimation(0, "work", true);
        }
        public void work_end()
        {
            this.animator.AnimationState.SetAnimation(0, "work_end", false);
            this.animator.AnimationState.SetAnimation(1, "default", true);
        }
        public void work_bad_end()
        {
            this.animator.AnimationState.SetAnimation(0, "work_bad_end", true);
        }
        public void Default()
        {
            this.animator.AnimationState.SetAnimation(0, "default", true);
        }
        public void escape()
        {
            this.animator.AnimationState.SetAnimation(0, "escaped", true);
        }
        public override bool HasDeadMotion()
        {
            return true;
        }
        public override void PlayDeadMotion()
        {
            base.PlayDeadMotion();
            this.animator.AnimationState.SetAnimation(0, "Dead", false);
        }
        public EGODispatcher script;
        public new SkeletonAnimation animator;
    }
}
