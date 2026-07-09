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

    public Sample GetRandomSample(Random random)
    {
        if (sampleList.Count == 1)
        return sampleList[0];
        else
        {
            int index = random.Next(sampleList.Count);
            return sampleList[index];
        }
    }

    public void AddSample(Sample sample)
    {
        sampleList.Add(sample);
    }

    public IEnumerator<Sample> GetEnumerator()
    {
        return sampleList.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
