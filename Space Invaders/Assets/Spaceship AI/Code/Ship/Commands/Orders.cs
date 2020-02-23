using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orders : Singleton<Orders>{

    public Order Move;
    public Order Patrol;
    public Order FollowMe;
    public Order Follow;
    public Order Idle;
    public Order DockAt;
    public Order Attack;

    private void Start()
    {
        if (Move == null)
            Debug.LogError("Move order not assigned to Orders Script!");
        if (Patrol == null)
            Debug.LogError("Patrol order not assigned to Orders Script!");
        if (FollowMe == null)
            Debug.LogError("FollowMe order not assigned to Orders Script!");
        if (Follow == null)
            Debug.LogError("Follow order not assigned to Orders Script!");
        if (Idle == null)
            Debug.LogError("Idle order not assigned to Orders Script!");
        if (DockAt == null)
            Debug.LogError("DockAt order not assigned to Orders Script!");
        if (Attack == null)
            Debug.LogError("Attack order not assigned to Orders Script!");
    }

}
