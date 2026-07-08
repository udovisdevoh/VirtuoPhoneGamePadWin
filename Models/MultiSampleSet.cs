package com.virtuophone.models;

import java.util.Iterator;
import java.util.Random;
import java.util.Vector;

public class MultiSampleSet implements Iterable<Sample> {
	private Vector<Sample> sampleList;
	
	public MultiSampleSet() {
		sampleList = new Vector<Sample>();
	}

	public Sample getRandomSample(Random random) {
		if (sampleList.size() == 1)
			return sampleList.get(0);
		else {
			int index = random.nextInt(sampleList.size());
			return sampleList.get(index);
		}
	}

	public void addSample(Sample sample) {
		sampleList.add(sample);
	}

	@Override
	public Iterator<Sample> iterator() {
		return sampleList.iterator();
	}

}
