using UnityEngine;
using System.Collections;

public class AutomaticVerticalSize : MonoBehaviour {

    public float childHeight = 35f;

	// Use this for initialization
	void Start () {
        this.AdjustSize();
	}

    public void AdjustSize()
    {
        Vector2 size = this.GetComponent<RectTransform>().sizeDelta;
        size.y = this.transform.childCount * this.childHeight;
        this.GetComponent<RectTransform>().sizeDelta = size;
    }
}
