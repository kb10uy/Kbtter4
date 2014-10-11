//This code is semi-auto-generated with regular expressions from jQuery.easing
//Under the MIT License.

using System;

namespace EasingSharp
{
    public delegate double EasingFunction(double time, double timeDuration, double startValue, double valueDuration);

    public static class Easing
    {
        public static double Simplify(this EasingFunction d, double r)
        {
            return d(r, 1, 0, 1);
        }

        public static double Linear(double time, double timeDuration, double startValue, double valueDuration)
        {
            return valueDuration * (time / timeDuration) + startValue;
        }

        public static double InQuad(double time, double timeDuration, double startValue, double valueDuration)
        {
            return valueDuration * (time /= timeDuration) * time + startValue;
        }

        public static double OutQuad(double time, double timeDuration, double startValue, double valueDuration)
        {
            return -valueDuration * (time /= timeDuration) * (time - 2) + startValue;
        }

        public static double InOutQuad(double time, double timeDuration, double startValue, double valueDuration)
        {
            if ((time /= timeDuration / 2) < 1) return valueDuration / 2 * time * time + startValue;
            return -valueDuration / 2 * ((--time) * (time - 2) - 1) + startValue;
        }

        public static double InCubic(double time, double timeDuration, double startValue, double valueDuration)
        {
            return valueDuration * (time /= timeDuration) * time * time + startValue;
        }

        public static double OutCubic(double time, double timeDuration, double startValue, double valueDuration)
        {
            return valueDuration * ((time = time / timeDuration - 1) * time * time + 1) + startValue;
        }

        public static double InOutCubic(double time, double timeDuration, double startValue, double valueDuration)
        {
            if ((time /= timeDuration / 2) < 1) return valueDuration / 2 * time * time * time + startValue;
            return valueDuration / 2 * ((time -= 2) * time * time + 2) + startValue;
        }

        public static double InQuart(double time, double timeDuration, double startValue, double valueDuration)
        {
            return valueDuration * (time /= timeDuration) * time * time * time + startValue;
        }

        public static double OutQuart(double time, double timeDuration, double startValue, double valueDuration)
        {
            return -valueDuration * ((time = time / timeDuration - 1) * time * time * time - 1) + startValue;
        }

        public static double InOutQuart(double time, double timeDuration, double startValue, double valueDuration)
        {
            if ((time /= timeDuration / 2) < 1) return valueDuration / 2 * time * time * time * time + startValue;
            return -valueDuration / 2 * ((time -= 2) * time * time * time - 2) + startValue;
        }

        public static double InQuint(double time, double timeDuration, double startValue, double valueDuration)
        {
            return valueDuration * (time /= timeDuration) * time * time * time * time + startValue;
        }

        public static double OutQuint(double time, double timeDuration, double startValue, double valueDuration)
        {
            return valueDuration * ((time = time / timeDuration - 1) * time * time * time * time + 1) + startValue;
        }

        public static double InOutQuint(double time, double timeDuration, double startValue, double valueDuration)
        {
            if ((time /= timeDuration / 2) < 1) return valueDuration / 2 * time * time * time * time * time + startValue;
            return valueDuration / 2 * ((time -= 2) * time * time * time * time + 2) + startValue;
        }

        public static double InSine(double time, double timeDuration, double startValue, double valueDuration)
        {
            return -valueDuration * Math.Cos(time / timeDuration * (Math.PI / 2)) + valueDuration + startValue;
        }

        public static double OutSine(double time, double timeDuration, double startValue, double valueDuration)
        {
            return valueDuration * Math.Sin(time / timeDuration * (Math.PI / 2)) + startValue;
        }

        public static double InOutSine(double time, double timeDuration, double startValue, double valueDuration)
        {
            return -valueDuration / 2 * (Math.Cos(Math.PI * time / timeDuration) - 1) + startValue;
        }

        public static double InExpo(double time, double timeDuration, double startValue, double valueDuration)
        {
            return (time == 0) ? startValue : valueDuration * Math.Pow(2, 10 * (time / timeDuration - 1)) + startValue;
        }

        public static double OutExpo(double time, double timeDuration, double startValue, double valueDuration)
        {
            return (time == timeDuration) ? startValue + valueDuration : valueDuration * (-Math.Pow(2, -10 * time / timeDuration) + 1) + startValue;
        }

        public static double InOutExpo(double time, double timeDuration, double startValue, double valueDuration)
        {
            if (time == 0) return startValue;
            if (time == timeDuration) return startValue + valueDuration;
            if ((time /= timeDuration / 2) < 1) return valueDuration / 2 * Math.Pow(2, 10 * (time - 1)) + startValue;
            return valueDuration / 2 * (-Math.Pow(2, -10 * --time) + 2) + startValue;
        }

        public static double InCircle(double time, double timeDuration, double startValue, double valueDuration)
        {
            return -valueDuration * (Math.Sqrt(1 - (time /= timeDuration) * time) - 1) + startValue;
        }

        public static double OutCircle(double time, double timeDuration, double startValue, double valueDuration)
        {
            return valueDuration * Math.Sqrt(1 - (time = time / timeDuration - 1) * time) + startValue;
        }

        public static double InOutCircle(double time, double timeDuration, double startValue, double valueDuration)
        {
            if ((time /= timeDuration / 2) < 1) return -valueDuration / 2 * (Math.Sqrt(1 - time * time) - 1) + startValue;
            return valueDuration / 2 * (Math.Sqrt(1 - (time -= 2) * time) + 1) + startValue;
        }

        public static double InElastic(double time, double timeDuration, double startValue, double valueDuration)
        {
            var s = 1.70158; var p = 0.0; var a = valueDuration;
            if (time == 0) return startValue; if ((time /= timeDuration) == 1) return startValue + valueDuration; if (p == 0) p = timeDuration * .3;
            if (a < Math.Abs(valueDuration))
            {
                a = valueDuration; s = p / 4;
            }
            else
            {
                s = p / (2 * Math.PI) * Math.Asin(valueDuration / a);
            }
            return -(a * Math.Pow(2, 10 * (time -= 1)) * Math.Sin((time * timeDuration - s) * (2 * Math.PI) / p)) + startValue;
        }

        public static double OutElastic(double time, double timeDuration, double startValue, double valueDuration)
        {
            var s = 1.70158; var p = 0.0; var a = valueDuration;
            if (time == 0) return startValue; if ((time /= timeDuration) == 1) return startValue + valueDuration; if (p == 0) p = timeDuration * .3;
            if (a < Math.Abs(valueDuration))
            {
                a = valueDuration; s = p / 4;
            }
            else
            {
                s = p / (2 * Math.PI) * Math.Asin(valueDuration / a);
            }
            return a * Math.Pow(2, -10 * time) * Math.Sin((time * timeDuration - s) * (2 * Math.PI) / p) + valueDuration + startValue;
        }

        public static double InOutElastic(double time, double timeDuration, double startValue, double valueDuration)
        {
            var s = 1.70158; var p = 0.0; var a = valueDuration;
            if (time == 0) return startValue; if ((time /= timeDuration / 2) == 2) return startValue + valueDuration; if (p == 0) p = timeDuration * (.3 * 1.5);
            if (a < Math.Abs(valueDuration))
            {
                a = valueDuration; s = p / 4;
            }
            else
            {
                s = p / (2 * Math.PI) * Math.Asin(valueDuration / a);
            }
            if (time < 1) return -.5 * (a * Math.Pow(2, 10 * (time -= 1)) * Math.Sin((time * timeDuration - s) * (2 * Math.PI) / p)) + startValue;
            return a * Math.Pow(2, -10 * (time -= 1)) * Math.Sin((time * timeDuration - s) * (2 * Math.PI) / p) * .5 + valueDuration + startValue;
        }

        public static double InBack(double time, double timeDuration, double startValue, double valueDuration)
        {
            return valueDuration * (time /= timeDuration) * time * ((2.70158) * time - 1.70158) + startValue;
        }

        public static double OutBack(double time, double timeDuration, double startValue, double valueDuration)
        {
            return valueDuration * ((time = time / timeDuration - 1) * time * ((2.70158) * time + 1.70158) + 1) + startValue;
        }

        public static double InOutBack(double time, double timeDuration, double startValue, double valueDuration)
        {
            var s = 1.70158;
            if ((time /= timeDuration / 2) < 1) return valueDuration / 2 * (time * time * (((s *= (1.525)) + 1) * time - s)) + startValue;
            return valueDuration / 2 * ((time -= 2) * time * (((s *= (1.525)) + 1) * time + s) + 2) + startValue;
        }

        public static double InBounce(double time, double timeDuration, double startValue, double valueDuration)
        {
            return valueDuration - OutBounce(timeDuration - time, 0, valueDuration, timeDuration) + startValue;
        }

        public static double OutBounce(double time, double timeDuration, double startValue, double valueDuration)
        {
            if ((time /= timeDuration) < (1 / 2.75))
            {
                return valueDuration * (7.5625 * time * time) + startValue;
            }
            else if (time < (2 / 2.75))
            {
                return valueDuration * (7.5625 * (time -= (1.5 / 2.75)) * time + .75) + startValue;
            }
            else if (time < (2.5 / 2.75))
            {
                return valueDuration * (7.5625 * (time -= (2.25 / 2.75)) * time + .9375) + startValue;
            }
            else
            {
                return valueDuration * (7.5625 * (time -= (2.625 / 2.75)) * time + .984375) + startValue;
            }
        }

        public static double InOutBounce(double time, double timeDuration, double startValue, double valueDuration)
        {
            if (time < timeDuration / 2) return InBounce(time * 2, 0, valueDuration, timeDuration) * .5 + startValue;
            return OutBounce(time * 2 - timeDuration, 0, valueDuration, timeDuration) * .5 + valueDuration * .5 + startValue;
        }
    }
}
