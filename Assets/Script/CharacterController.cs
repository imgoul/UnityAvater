﻿using UnityEngine;
using System.Collections;

public class UCharacterController
{
    /// <summary>
    /// GameObject reference
    /// </summary>
    public GameObject Instance = null;

    public GameObject WeaponInstance = null;

    /// <summary>
    /// Equipment informations
    /// </summary>
    public string skeleton;

    public string equipment_head;
    public string equipment_chest;
    public string equipment_hand;
    public string equipment_feet;

    /// <summary>
    /// The unique id in the scene
    /// </summary>
    public int index;

    /// <summary>
    /// Other vars
    /// </summary>
    public bool rotate = false;

    public int animationState = 0;

    private Animation animationController = null;

    public UCharacterController(int index, string skeleton, string weapon, string head, string chest, string hand,
        string feet, bool combine = false)
    {
        string str =
            string.Format("创建人物模型: \n index：{0} \n skeleton：{1} \n weapon：{2} \n head：{3} \n chest：{4} \n hand：{5} \n feet：{6} \n combine:{7}",
                index, skeleton, weapon, head, chest, hand, feet, combine);
        Debug.Log(str);
        
        //加载人物骨骼预设，骨骼预设没有网格
        Object res = Resources.Load("Prefab/" + skeleton);
        //实例化骨骼
        this.Instance = GameObject.Instantiate(res) as GameObject;
        
        this.index = index;
        this.skeleton = skeleton;
        this.equipment_head = head;
        this.equipment_chest = chest;
        this.equipment_hand = hand;
        this.equipment_feet = feet;

        string[] equipments = new string[4];
        equipments[0] = head;
        equipments[1] = chest;
        equipments[2] = hand;
        equipments[3] = feet;

        // 创建需要用到的皮肤网格渲染器-SkinnedMeshRenderer
        SkinnedMeshRenderer[] meshes = new SkinnedMeshRenderer[4];
        
        
        GameObject[] objects = new GameObject[4];
        for (int i = 0; i < equipments.Length; i++)
        {
            res = Resources.Load("Prefab/" + equipments[i]);
            objects[i] = GameObject.Instantiate(res) as GameObject;
            
            //获取实例化的人物各个部件中的皮肤网格渲染器SkinnedMeshRenderer
            meshes[i] = objects[i].GetComponentInChildren<SkinnedMeshRenderer>();
        }

        // 合并人物的各个部分的网格为一个大网格
        App.Game.CharacterMgr.CombineSkinnedMgr.CombineObject(Instance, meshes, combine);

        // 删除加载出的资源
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject.DestroyImmediate(objects[i].gameObject);
        }

        // 创建武器
        res = Resources.Load("Prefab/" + weapon);
        WeaponInstance = GameObject.Instantiate(res) as GameObject;

        Transform[] transforms = Instance.GetComponentsInChildren<Transform>();
        foreach (Transform joint in transforms)
        {
            if (joint.name == "weapon_hand_r")
            {
                // find the joint (need the support of art designer)
                WeaponInstance.transform.parent = joint.gameObject.transform;
                break;
            }
        }

        // Init weapon relative informations
        WeaponInstance.transform.localScale = Vector3.one;
        WeaponInstance.transform.localPosition = Vector3.zero;
        WeaponInstance.transform.localRotation = Quaternion.identity;

        // Only for display
        animationController = Instance.GetComponent<Animation>();
        PlayStand();
    }

    public void ChangeHeadEquipment(string equipment, bool combine = false)
    {
        ChangeEquipment(0, equipment, combine);
    }

    public void ChangeChestEquipment(string equipment, bool combine = false)
    {
        ChangeEquipment(1, equipment, combine);
    }

    public void ChangeHandEquipment(string equipment, bool combine = false)
    {
        ChangeEquipment(2, equipment, combine);
    }

    public void ChangeFeetEquipment(string equipment, bool combine = false)
    {
        ChangeEquipment(3, equipment, combine);
    }

    public void ChangeWeapon(string weapon)
    {
        Object res = Resources.Load("Prefab/" + weapon);
        GameObject oldWeapon = WeaponInstance;
        WeaponInstance = GameObject.Instantiate(res) as GameObject;
        WeaponInstance.transform.parent = oldWeapon.transform.parent;
        WeaponInstance.transform.localPosition = Vector3.zero;
        WeaponInstance.transform.localScale = Vector3.one;
        WeaponInstance.transform.localRotation = Quaternion.identity;

        GameObject.Destroy(oldWeapon);
    }

    public void ChangeEquipment(int index, string equipment, bool combine = false)
    {
        switch (index)
        {
            case 0:
                equipment_head = equipment;
                break;
            case 1:
                equipment_chest = equipment;
                break;
            case 2:
                equipment_hand = equipment;
                break;
            case 3:
                equipment_feet = equipment;
                break;
        }

        string[] equipments = new string[4];
        equipments[0] = equipment_head;
        equipments[1] = equipment_chest;
        equipments[2] = equipment_hand;
        equipments[3] = equipment_feet;

        Object res = null;
        SkinnedMeshRenderer[] meshes = new SkinnedMeshRenderer[4];
        GameObject[] objects = new GameObject[4];
        for (int i = 0; i < equipments.Length; i++)
        {
            res = Resources.Load("Prefab/" + equipments[i]);
            objects[i] = GameObject.Instantiate(res) as GameObject;
            meshes[i] = objects[i].GetComponentInChildren<SkinnedMeshRenderer>();
        }

        App.Game.CharacterMgr.CombineSkinnedMgr.CombineObject(Instance, meshes, combine);

        for (int i = 0; i < objects.Length; i++)
        {
            GameObject.DestroyImmediate(objects[i].gameObject);
        }
    }

    public void PlayStand()
    {
        animationController.wrapMode = WrapMode.Loop;
        animationController.Play("breath");
        animationState = 0;
    }

    public void PlayAttack()
    {
        animationController.wrapMode = WrapMode.Once;
        animationController.PlayQueued("attack1");
        animationController.PlayQueued("attack2");
        animationController.PlayQueued("attack3");
        animationController.PlayQueued("attack4");
        animationState = 1;
    }

    // Update is called once per frame
    public void Update()
    {
        if (animationState == 1)
        {
            if (!animationController.isPlaying)
            {
                PlayAttack();
            }
        }

        if (rotate)
        {
            Instance.transform.Rotate(new Vector3(0, 90 * Time.deltaTime, 0));
        }
    }
}