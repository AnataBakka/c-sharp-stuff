using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

namespace UI {
    public class Health : MonoBehaviour {
        // Start is called before the first frame update
        [SerializeField]
        private GameObject cube_health;

        private GameObject cube_max_health;
        private GameObject cube_current_health;
        private float character_height;

        private float current_health = 100;

        public float Current_health {
            get {
                return current_health;
            }

            set {
                if (current_health <= 0) {
                    return;
                }

                if (value <= 0) {
                    gameObject.SetActive(false);
                    Destroy(cube_max_health);
                    Destroy(cube_current_health);
                    value = 0;
                }

                float dmg = current_health - value;
                current_health = value;

                cube_current_health.transform.localScale -= Vector3.right * (dmg / 100f);
                cube_current_health.transform.Translate(Vector3.left * (cube_max_health.transform.localScale.x * dmg / 200f));
                
            }
        }

        void Start() {
            character_height = GetComponent<CapsuleCollider>().height;
            cube_max_health = Instantiate(cube_health) as GameObject;
            cube_current_health = Instantiate(cube_health) as GameObject;
            cube_current_health.transform.parent = cube_max_health.transform;
            cube_current_health.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.2f);
        }

        void Update() {
            cube_max_health.transform.position = transform.position + Camera.main.transform.up * (character_height + 0.5f);
            cube_max_health.transform.LookAt(Camera.main.transform, Vector3.down);
        }
    }
}