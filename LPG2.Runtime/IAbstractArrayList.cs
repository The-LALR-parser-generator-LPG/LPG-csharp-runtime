namespace LPG2.Runtime
{

    public interface IAbstractArrayList<T > where T:  IAst
    {
    public int size();
    public T getElementAt(int i);
    public ArrayList<T> getList();
    public void add(T elt);
    public ArrayList<T> getAllChildren();
    }
}