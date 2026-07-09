using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;





public class MultiSampleSet : IEnumerable<Sample>
{
    private List<Sample> sampleList;

    public MultiSampleSet()
    {
        sampleList = new List<Sample>();
    }

    public Sample getRandomSample(Random random)
    {
        if (sampleList.Count == 1)
        return sampleList[0];
        else
        {
            int index = random.nextInt(sampleList.Count);
            return sampleList[index];
        }
    }

    public void addSample(Sample sample)
    {
        sampleList.add(sample);
    }

    public IEnumerator<Sample> iterator()
    {
        return sampleList.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
