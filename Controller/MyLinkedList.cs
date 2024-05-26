using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRPG
{
    public class MyLinkedList
    {
        public int Count { get; private set; }
        public MyLinkedListNode Last { get; private set; }
        public MyLinkedList(Point point)
        {
            Count = 1;
            Last = new MyLinkedListNode(point);
        }

        public MyLinkedList()
        {
            Count = 0;
        }

        public void AddLast(Point point)
        {
            Last = new MyLinkedListNode(point, Last);
            Count++;
        }

        public void SetLast(MyLinkedListNode last, int count)
        {
            Last = last;
            Count = count;
        }

        public Point[] ToArray()
        {
            var result = new Point[Count];
            var currentNode = Last;
            for (int i = Count - 1; i >= 0; i--)
            {
                result[i] = currentNode.Value;
                currentNode = currentNode.Previous;
            }
            return result;
        }
    }

    public class MyLinkedListNode
    {
        public Point Value { get; private set; }
        public MyLinkedListNode Previous { get; private set; }
        public MyLinkedListNode(Point point, MyLinkedListNode previous = null)
        {
            Value = point;
            Previous = previous;
        }
    }
}
