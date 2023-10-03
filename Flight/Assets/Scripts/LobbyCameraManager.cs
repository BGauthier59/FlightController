using System;
using System.Threading.Tasks;
using UnityEngine;

public class LobbyCameraManager : MonoSingleton<LobbyCameraManager>
{
    [SerializeField] private Transform camera;

    [Serializable]
    public struct Bookmark
    {
        public Transform position, control_1, control_2;
        public Vector3 rotation;
    }

    private Bookmark currentBookmark;
    [SerializeField] private Bookmark[] bookmarks;

    private void Start()
    {
        currentBookmark = bookmarks[0];
        camera.position = currentBookmark.position.position;
        camera.eulerAngles = currentBookmark.position.eulerAngles;
    }

    public async Task MoveToBookmark(int index, float duration)
    {
        Bookmark bookmark = bookmarks[index];

        Vector3 p1, p2, p3, p4;
        p1 = currentBookmark.position.position;
        p2 = bookmark.control_1.position;
        p3 = bookmark.control_2.position;
        p4 = bookmark.position.position;

        Vector3 r1, r2;
        r1 = currentBookmark.rotation;
        r2 = bookmark.rotation;

        float timer = 0;

        while (timer < duration)
        {
            camera.position = Ex.CubicBeziersCurve(p1, p2, p3, p4, timer / duration);
            camera.eulerAngles = Vector3.Slerp(r1, r2, timer / duration);
            await Task.Yield();
            timer += Time.deltaTime;
        }

        camera.position = bookmark.position.position;
        camera.eulerAngles = bookmark.rotation;

        currentBookmark = bookmark;
    }
}