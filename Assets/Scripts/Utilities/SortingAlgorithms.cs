using UnityEngine;

public class SortingAlgorithms : MonoBehaviour
{
  // Partition function
  static int Partition(int[] arr, int low, int high)
  {

    // Choose the pivot
    int pivot = arr[high];

    // Index of smaller element and indicates 
    // the right position of pivot found so far
    int i = low - 1;

    // Traverse arr[low..high] and move all smaller
    // elements to the left side. Elements from low to 
    // i are smaller after every iteration
    for (int j = low; j <= high - 1; j++)
    {
      if (arr[j] < pivot)
      {
        i++;
        Swap(arr, i, j);
      }
    }

    // Move pivot after smaller elements and
    // return its position
    Swap(arr, i + 1, high);
    return i + 1;
  }
  // Partition function
  static int Partition(float[] arr, int low, int high)
  {

    // Choose the pivot
    float pivot = arr[high];

    // Index of smaller element and indicates 
    // the right position of pivot found so far
    int i = low - 1;

    // Traverse arr[low..high] and move all smaller
    // elements to the left side. Elements from low to 
    // i are smaller after every iteration
    for (int j = low; j <= high - 1; j++)
    {
      if (arr[j] < pivot)
      {
        i++;
        Swap(arr, i, j);
      }
    }

    // Move pivot after smaller elements and
    // return its position
    Swap(arr, i + 1, high);
    return i + 1;
  }

  // Swap function
  static void Swap(int[] arr, int i, int j)
  {
    int temp = arr[i];
    arr[i] = arr[j];
    arr[j] = temp;
  }
  // Swap function
  static void Swap(float[] arr, int i, int j)
  {
    float temp = arr[i];
    arr[i] = arr[j];
    arr[j] = temp;
  }

  // The QuickSort function implementation
  static void QuickSort(int[] arr, int low, int high)
  {
    if (low < high)
    {

      // pi is the partition return index of pivot
      int pi = Partition(arr, low, high);

      // Recursion calls for smaller elements
      // and greater or equals elements
      QuickSort(arr, low, pi - 1);
      QuickSort(arr, pi + 1, high);
    }
  }
  // The QuickSort function implementation
  public static void QuickSort(float[] arr, int low, int high)
  {
    if (low < high)
    {

      // pi is the partition return index of pivot
      int pi = Partition(arr, low, high);

      // Recursion calls for smaller elements
      // and greater or equals elements
      QuickSort(arr, low, pi - 1);
      QuickSort(arr, pi + 1, high);
    }
  }
}
