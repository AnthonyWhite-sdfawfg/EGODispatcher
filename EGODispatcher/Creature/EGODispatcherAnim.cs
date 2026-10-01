using System;
using Spine.Unity;

namespace Creature
{
    /// <summary>
    /// [EGODispatcher] 的 Spine 动画驱动脚本。
    /// <para>由 <see cref="EGODispatcher.OnViewInit"/> 在视图初始化时创建并注入逻辑脚本引用，
    /// 随后由逻辑侧（含 <c>CreatureTools</c> 中的协程）通过 <c>animscript</c> 字段间接播放动画。</para>
    /// <para>动画名需与 Spine 资源中的名称一致：<c>default</c>、<c>work</c>、<c>work_end</c>、
    /// <c>work_bad_end</c>、<c>escaped</c>、<c>Dead</c>。</para>
    /// </summary>
    public class EGODispatcherAnim : CreatureAnimScript
    {
        /// <summary>
        /// 绑定逻辑脚本并初始化动画组件，随后自动进入待机动画。
        /// </summary>
        /// <param name="script">对应的异想体逻辑脚本</param>
        public void SetScript(EGODispatcher script)
        {
            this.script = script;
            this.animator = base.gameObject.GetComponent<SkeletonAnimation>();
            this.animator.AnimationState.SetAnimation(0, "default", true);
        }

        /// <summary>[动画] 工作中，循环播放</summary>
        public void work()
        {
            this.animator.AnimationState.SetAnimation(0, "work", true);
        }

        /// <summary>
        /// [动画] 工作正常结束。
        /// <para>在轨道 0 播放一次性的结束动作，同时在轨道 1 叠加循环待机动作作为过渡。</para>
        /// </summary>
        public void work_end()
        {
            this.animator.AnimationState.SetAnimation(0, "work_end", false);
            this.animator.AnimationState.SetAnimation(1, "default", true);
        }

        /// <summary>[动画] 工作以负面结果结束（如员工死亡、工作失败）</summary>
        public void work_bad_end()
        {
            this.animator.AnimationState.SetAnimation(0, "work_bad_end", true);
        }

        /// <summary>[动画] 回到待机状态</summary>
        public void Default()
        {
            this.animator.AnimationState.SetAnimation(0, "default", true);
        }

        /// <summary>[动画] 出逃状态，循环播放</summary>
        public void escape()
        {
            this.animator.AnimationState.SetAnimation(0, "escaped", true);
        }

        /// <summary>
        /// 声明本异想体拥有死亡动作。
        /// </summary>
        /// <returns>恒为 true</returns>
        public override bool HasDeadMotion()
        {
            return true;
        }

        /// <summary>
        /// 播放死亡动作（一次性，不循环）。
        /// </summary>
        public override void PlayDeadMotion()
        {
            base.PlayDeadMotion();
            this.animator.AnimationState.SetAnimation(0, "Dead", false);
        }

        /// <summary>对应的异想体逻辑脚本，由 <see cref="SetScript"/> 注入</summary>
        public EGODispatcher script;
        /// <summary>Spine 动画状态机（隐藏基类同名成员，因此使用 <c>new</c> 修饰）</summary>
        public new SkeletonAnimation animator;
    }
}
