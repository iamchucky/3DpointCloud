using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.DataInterfaces;
using System.Collections;

namespace Magic.Common.DataStructures
{
    /// <summary>
    /// Non-Thread Safe Event Queue for Timestamped Items
    /// </summary>
    public class SynchronizedEventQueueCQ
    {
        double maxAgeDifference = 1.000; //seconds
        ITimestampedEventQueueItem[] events;
        int head = 0;
        int tail = 0;
        int length = 0;

        public SynchronizedEventQueueCQ(int capacity)
        {
            events = new ITimestampedEventQueueItem[capacity];
        }
        public void Clear()
        {
            length = 0;
            head = 0; 
            tail = 0;
        }
        public void AddEvent(ITimestampedEventQueueItem item)
        {
            //find the index by doing a simple linear search

            //the earliest items are at the top of this queue and the latest items on the bottom. 
            //the goal is to start from the bottom and insert right after the first item that is 
            //before us. 

            if (length >= events.Length)
            {
                Console.WriteLine("Circular queue is full.");
                return;
            }

            //add the item            
            events[tail] = item;
            if (tail == events.Length - 1) tail = 0;
            else tail++;            
            length++;

            //else
            //{
            //    //validate the measurement...
            //    //if (Math.Abs(events[events.Count - 1].TimeStamp - item.TimeStamp) > maxAgeDifference)
            //    //{
            //    //    Console.WriteLine("WARNING: ERRONEOUS MEASUREMENT TIMESTAMP!!! Last Measurement TS: " + events[events.Count - 1].TimeStamp + " Recieved: " + item.TimeStamp);
            //    //    events.Clear();
            //    //    return;
            //    //}

            //    for (int i = events.Count - 1; i >= 0; i--)
            //    {
            //        if (item.CompareTo(events[index]) < 0) //item is lt events[index]
            //        {
            //            index = i ;
            //            break;
            //        }
            //    }
            //    events.Insert(index, item);
            //}

        }

        public int NumItems()
        {
            return length;
            //return Math.Max (events.Count-5,0);
        }

        public ITimestampedEventQueueItem Peek(double curTS)
        {
            ITimestampedEventQueueItem ret;
            if (length == 0)
            {
                Console.WriteLine("Circular queue is empty.");
                ret = null;
            }
            else
            {
                ret = events[head];
            }
            return ret;
        }
        public ITimestampedEventQueueItem Pop(double curTS)
        {
            ITimestampedEventQueueItem ret;
            if (length == 0)
            {
                Console.WriteLine("Circular queue is empty.");
                ret = null;
            }
            else
            {
                ret = events[head];
                events[head] = null;
                length--;
                if (head == events.Length-1)
                    head = 0;
                else
                    head++;
            }
            return ret;


        }
    }
}
