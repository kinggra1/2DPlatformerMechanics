﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrowable {

    void Grow();
    bool CanBeWatered();
    void Water();  
}
