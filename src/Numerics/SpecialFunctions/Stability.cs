// <copyright file="Stability.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics
{
    using System;

    public partial class SpecialFunctions
    {
        /// <summary>
        /// Numerically stable exponential minus one, i.e. <code>x -> exp(x)-1</code>
        /// </summary>
        /// <param name="power">A number specifying a power.</param>
        /// <returns>Returns <code>exp(power)-1</code>.</returns>
        public static double ExponentialMinusOne(double power)
        {
            double x = Math.Abs(power);
            if (x > 0.1)
            {
                return Math.Exp(power) - 1.0;
            }

            if (x < x.PositiveEpsilonOf())
            {
                return x;
            }

            // Series Expansion to x^k / k!
            int k = 0;
            double term = 1.0;
            return Series(
                () =>
                {
                    k++;
                    term *= power;
                    term /= k;
                    return term;
                }
                );
        }

        /// <summary>
        /// Numerically stable hypotenuse of a right angle triangle, i.e. <code>(a,b) -> sqrt(a^2 + b^2)</code>
        /// </summary>
        /// <param name="a">The length of side a of the triangle.</param>
        /// <param name="b">The length of side b of the triangle.</param>
        /// <returns>Returns <code>sqrt(a<sup>2</sup> + b<sup>2</sup>)</code> without underflow/overflow.</returns>
        public static double Hypotenuse(double a, double b)
        {
            if (Math.Abs(a) > Math.Abs(b))
            {
                double r = b / a;
                return Math.Abs(a) * Math.Sqrt(1 + (r * r));
            }

            if (b != 0.0)
            {
                // NOTE (ruegg): not "!b.AlmostZero()" to avoid convergence issues (e.g. in SVD algorithm)
                double r = a / b;
                return Math.Abs(b) * Math.Sqrt(1 + (r * r));
            }

            return 0d;
        }

        /// <summary>
        /// Numerically stable series summation
        /// </summary>
        /// <param name="nextSummand">provides the summands sequentially</param>
        /// <returns>Sum</returns>
        private static double Series(Func<double> nextSummand)
        {
            double compensation = 0.0;
            double current;
            double factor = 1 << 16;

            double sum = nextSummand();

            do
            {
                // Kahan Summation
                // NOTE (ruegg): do NOT optimize. Now, how to tell that the compiler?
                current = nextSummand();
                double y = current - compensation;
                double t = sum + y;
                compensation = t - sum;
                compensation -= y;
                sum = t;
            }
            while (Math.Abs(sum) < Math.Abs(factor * current));

            return sum;
        }
    }
}