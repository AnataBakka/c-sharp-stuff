using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

namespace Movement {
    public class NewAIMovement : Movement {
        [SerializeField]
        private float speed = 1;
        private float temp_speed;

        private GameObject player;
        private ThirdPersonCharacter AI;
        private float height;
        private float width;

        private GameObject[] rotations;
        private int angles = 1;
        private int rotations_length;
        private Vector3 ignore_y_axis = new Vector3(1, 0, 1);

        private Vector3 target;
        private Vector3 furthest_target;
        private Vector3 temp_furthest_target;
        private bool player_close_to_ai = false;
        private bool target_reached = false;

        void Start() {
            player = GameObject.Find("Player");
            target = Vector3.Scale(transform.position, ignore_y_axis);
            furthest_target = Vector3.Scale(transform.position, ignore_y_axis);
            temp_furthest_target = furthest_target;
            height = GetComponent<CapsuleCollider>().height;
            width = GetComponent<CapsuleCollider>().radius * 2;
            temp_speed = speed;

            rotations_length = 360 / angles;
            rotations = new GameObject[rotations_length];

            for (int i = 0; i < rotations_length; i++) {
                rotations[i] = new GameObject();
                rotations[i].transform.Rotate(new Vector3(0, i * angles, 0));
            }

            AI = GetComponent<ThirdPersonCharacter>();
        }

        void Update() {
            if (player) {
                //Finds a target point to move to that's most away from the player.
                float longest_distance = (furthest_target - Vector3.Scale(player.transform.position, ignore_y_axis)).magnitude + width; //the default is so that it keeps the same target, unless it finds a better one
                Vector3 temp_target;
                float temp_distance;

                if ((Vector3.Scale(player.transform.position, ignore_y_axis) - transform.position).magnitude < 10) {
                    player_close_to_ai = true;
                }
                else {
                    player_close_to_ai = false;
                }

                //find the furthest target from player
                for (int i = 0; i < rotations_length; i++) {
                    temp_target = find_target(rotations[i].transform.forward, width * 1000); //put a high value

                    //the ai can only find targets that are away from the player until the player gets too close.
                    //then the ai is granted the right to find a target over the player.
                    if (!player_close_to_ai) {
                        float player_to_target_angle = Vector3.SignedAngle(Vector3.Scale(transform.position - player.transform.position, ignore_y_axis), temp_target - Vector3.Scale(player.transform.position, ignore_y_axis), Vector3.up);

                        if (player_to_target_angle < -90 || player_to_target_angle > 90) {
                            continue;
                        }
                    }

                    temp_distance = (temp_target - Vector3.Scale(player.transform.position, ignore_y_axis)).magnitude;

                    if (temp_distance > longest_distance) {
                        target_reached = false;
                        longest_distance = temp_distance;
                        furthest_target = temp_target;
                        temp_furthest_target = furthest_target;
                    }
                }

                //to make sure the ai doesn't stand still
                if (target_reached) {
                    target_reached = false;

                    //making sure the ai stays in range of target
                    if ((furthest_target - Vector3.Scale(transform.position, ignore_y_axis)).magnitude > width * 20) {
                        temp_furthest_target = Vector3.Scale(transform.position, ignore_y_axis) + (furthest_target - Vector3.Scale(transform.position, ignore_y_axis)).normalized * width * 10;
                    }
                    else {
                        //width*10 is to make sure the ai moves for a while before changing direction
                        temp_furthest_target = Vector3.Scale(transform.position, ignore_y_axis) + rotations[Random.Range(0, rotations_length)].transform.forward * width * 10;
                    }
                }

                longest_distance = (target - Vector3.Scale(player.transform.position, ignore_y_axis)).magnitude - (target - temp_furthest_target).magnitude + width;
                //find the furthest target from player and closest to the above target
                for (int i = 0; i < rotations_length; i++) {
                    temp_target = find_target(rotations[i].transform.forward, width * 10);

                    temp_distance = (temp_target - Vector3.Scale(player.transform.position, ignore_y_axis)).magnitude - (temp_target - temp_furthest_target).magnitude;

                    if (temp_distance > longest_distance) {
                        longest_distance = temp_distance;
                        target = temp_target;
                    }
                }
            }
        }

        Vector3 find_target(Vector3 direction, float distance) {
            //Return the closest impassable wall in the direction x distance
            //or direction x distance if none found
            RaycastHit[] hits = Physics.RaycastAll(transform.position + transform.up * height / 2f, direction, distance, -1, QueryTriggerInteraction.Ignore);
            bool wall_found = false;
            RaycastHit closest_hit = new RaycastHit(); //cannot be null
            float closest_hit_length = Mathf.Infinity;

            for (int i = 0; i < hits.Length; i++) {
                RaycastHit hit = hits[i];

                if (hit.rigidbody == null || hit.rigidbody.isKinematic == false) {
                    continue;
                }

                wall_found = true;

                float temp_hit_length = (hit.point - transform.position).magnitude;

                if (temp_hit_length < closest_hit_length) {
                    closest_hit = hit;
                    closest_hit_length = temp_hit_length;
                }
            }

            if (wall_found) {
                return Vector3.Scale(closest_hit.point, ignore_y_axis);
            }

            return Vector3.Scale(transform.position, ignore_y_axis) + direction * distance;
        }

        void FixedUpdate() {
            if (player) {
                if ((target - Vector3.Scale(transform.position, ignore_y_axis)).magnitude <= width) {
                    temp_speed = 0;
                    target_reached = true;
                }
                else {
                    temp_speed = speed;
                }

                AI.Move((target - Vector3.Scale(transform.position, ignore_y_axis)).normalized * temp_speed, false, false);
            }
        }

        void OnDisable() {
            for (int i = 0; i < rotations_length; i++) {
                Destroy(rotations[i]);
            }
        }
    }
}
