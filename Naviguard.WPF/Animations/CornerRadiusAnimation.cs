using System.Windows;
using System.Windows.Media.Animation;

namespace Naviguard.WPF.Animations
{
    public class CornerRadiusAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType => typeof(CornerRadius);

        public CornerRadius From
        {
            get => (CornerRadius)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(CornerRadius), typeof(CornerRadiusAnimation));

        public CornerRadius To
        {
            get => (CornerRadius)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(CornerRadius), typeof(CornerRadiusAnimation));

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            var fromVal = From;
            var toVal = To;

            if (animationClock.CurrentProgress == null)
                return fromVal;

            double progress = animationClock.CurrentProgress.Value;

            // Si From no está definido, usar valor original (ej: CornerRadius actual del control)
            if (ReadLocalValue(FromProperty) == DependencyProperty.UnsetValue)
                fromVal = (CornerRadius)defaultOriginValue;

            if (ReadLocalValue(ToProperty) == DependencyProperty.UnsetValue)
                toVal = (CornerRadius)defaultDestinationValue;

            return new CornerRadius(
                fromVal.TopLeft + (toVal.TopLeft - fromVal.TopLeft) * progress,
                fromVal.TopRight + (toVal.TopRight - fromVal.TopRight) * progress,
                fromVal.BottomRight + (toVal.BottomRight - fromVal.BottomRight) * progress,
                fromVal.BottomLeft + (toVal.BottomLeft - fromVal.BottomLeft) * progress
            );
        }


        protected override Freezable CreateInstanceCore() => new CornerRadiusAnimation();
    }
}
