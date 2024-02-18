using System;

namespace BetaManager.ViewModels
{
    public class ChangeChildViewMessage
    {
        public Type TargetViewModelType { get; }

        public ChangeChildViewMessage ( Type targetViewModelType )
        {
            TargetViewModelType = targetViewModelType;
        }
    }
}