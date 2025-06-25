using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convention
{
    namespace NTree
    {
        public class NTreeNode<T>
        {
            public NTreeNode<T> Parent;
            public NTreeNode<T> Child;
            public NTreeNode<T> Right;
            public T Data;

            public int BranchSize()
            {
                if (Parent == null)
                    return -1;
                var left =  Parent.Child;
                int result = 0;
                while (left != null)
                    result++;
                return result;
            }

            public int BranchLeftCount()
            {
                if (Parent == null)
                    return -1;
                var left = Parent.Child;
                int result = 0;
                while (left != this)
                    result++;
                return result;
            }

            public int BranchRightSize()
            {
                var left = this;
                int result = 0;
                while (left != null)
                    result++;
                return result;
            }
        }

        public class NTree<T>
        {
            internal NTreeNode<T> Root;

            public bool IsEmpty() => Root == null;

            public NTreeNode<T> UpdateRoot(NTreeNode<T> node)
            {
                var oldRoot = Root;
                Root = node;
                return oldRoot;
            }
        }
    }
}
