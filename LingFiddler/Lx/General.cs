using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lx
{

    public class Phonology { }


    public class Concept
    {
        public string Symbol { get; set; }
    }

    public class ConceptSet : HashSet<Concept>
    {
    }

    public class Feature { }

    public class Paradigm
    {
        // geometry of inflectional features
        // functionally the mapping of Pragma to Morph (one to many)
        // function(Pragma, Features) => Morph
    }

    public class ParadigmSet : HashSet<Paradigm>
    {
    }


}

public static class Extensions
{
    public static T[] Slice<T>(this T[] source, int start)
    {


        if (source != null && start < source.Length)
        {
            if (start < 0)
                start = 0;

            T[] slice = new T[source.Length - start];

            for (int i = 0; i + start < source.Length; i++)
            {
                slice[i] = source[i + start];
            }

            return slice;
        }
        else
        {
            return null;
        }
    }

    public static T[] Slice<T>(this T[] source, int start, int length)
    {
        if (length < 0)
            return null;

        T[] slice = new T[length];

        for (int i = 0; i < length; i++)
        {
            slice[i] = source[i + start];
        }

        return slice;
    }

    public static int Overlap<T>(this T[] rightArray, T[] leftArray)
    {
        int index = 0;

        if (
            rightArray != null
            && leftArray != null
            && rightArray.Length > 0
            && leftArray.Length > 0
            //&& rightArray.Intersect(leftArray).ToArray().Length > 0
            )
        {
            for (int i = 0; i < rightArray.Length && i < leftArray.Length; i++)
            {
                var rightStub = rightArray.Slice(0, i + 1);
                var leftStub = leftArray.Slice(leftArray.Length - 1 - i);

                if (rightStub.SequenceEqual(leftStub))
                {
                    index = i + 1;
                }
            }
        }

        return index;
    }
}

