using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LingFiddlerTests
{
    public class IntTests
    {
        public const int Iters = 10000;

        public enum Stuff { Plum, Dog, Telephone, Assassin }

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void ShortTest()
        {
            var random = new Random();
            var list = new List<short>();
            var dict = new Dictionary<short, string>();
            short number = Iters;

            for (short i = 0; i < Iters; i++)
            {
                list.Add((short)(i - number));
            }

            foreach (var num in list)
            {
                dict.Add(num, ((Stuff)random.Next(4)).ToString());
            }

            for (short i = 10; i < 100; i++)
            {
                var first = list[i];
                var second = dict[(short)(i - number)];

                Console.WriteLine(string.Format("{0} - {1}", first, second));
            }

            Assert.Pass();
        }

        [Test]
        public void IntTest()
        {
            var random = new Random();
            var list = new List<int>();
            var dict = new Dictionary<int, string>();
            int number = Iters;

            for (int i = 0; i < Iters; i++)
            {
                list.Add(i - number);
            }

            foreach (var num in list)
            {
                dict.Add(num, ((Stuff)random.Next(4)).ToString());
            }

            for (int i = 10; i < 100; i++)
            {
                var first = list[i];
                var second = dict[i - number];

                Console.WriteLine(string.Format("{0} - {1}", first, second));
            }

            Assert.Pass();
        }
        /*
         * 
         */
    }
}