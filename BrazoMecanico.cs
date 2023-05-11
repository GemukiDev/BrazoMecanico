using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Resultado {
    public bool correcto;
    public Interactuable interactuable;
    public Vector3 posicion;
}

public class BrazoMecanico : MonoBehaviour {

    public float vel = 1f;
    public float recogidoY = 2f;
    private Recogible objetoAgarrado;

    void Update() {
        if(Input.GetMouseButtonDown(0)) {
            Resultado resultado = HizoClickEnInteractuable();
            if(resultado.correcto) {
                if(resultado.interactuable is Recogible) {
                    StartCoroutine(AgarrarRecogible(resultado.interactuable as Recogible));
                } else if(resultado.interactuable is Suelo && objetoAgarrado != null){
                    StartCoroutine(SoltarRecogible(resultado.posicion + Vector3.up*0.5f));
                }
            }
        }
    }

    private Resultado HizoClickEnInteractuable() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Resultado resultado = new Resultado();

        if (Physics.Raycast(ray, out hit)) {
            resultado.correcto = true;
            Interactuable interactuable = hit.collider.gameObject.GetComponent<Interactuable>();
            if(interactuable != null) {
                resultado.interactuable = interactuable;
            }
            resultado.posicion = hit.point;
        }
        return resultado;
    }

    private IEnumerator AgarrarRecogible(Recogible recogible) {
        Vector3 posicion = recogible.transform.position;
        yield return Mover(new Vector3(posicion.x, transform.position.y, posicion.z));
        yield return Mover(new Vector3(posicion.x, posicion.y, posicion.z));

        Coger(recogible);
        yield return Mover(new Vector3(posicion.x, recogidoY, posicion.z));
    }

    private IEnumerator Mover(Vector3 posicion) {
        while(true) {
            Vector3 direccion = (posicion - transform.position).normalized;
            Debug.DrawLine(transform.position, transform.position + direccion);
            Vector3 movimientoFotograma = direccion * vel * Time.deltaTime;
            if(movimientoFotograma.magnitude < Vector3.Distance(transform.position, posicion)) {
                transform.position += movimientoFotograma;
                yield return null;
            } else {
                transform.position = posicion;
                break;
            }
        }
    }

    private IEnumerator SoltarRecogible(Vector3 posicion) {
        yield return Mover(new Vector3(posicion.x, transform.position.y, posicion.z));
        yield return Mover(new Vector3(posicion.x, 0.2f, posicion.z));

        Soltar();
        yield return Mover(new Vector3(posicion.x, recogidoY, posicion.z));
    }

    private void Coger(Recogible recogible) {
        ParentConstraint constraint = recogible.gameObject.AddComponent<ParentConstraint>();
        ConstraintSource source = new ConstraintSource();
        source.sourceTransform = transform;
        source.weight = 1.0f;
        constraint.AddSource(source);
        constraint.constraintActive = true;

        objetoAgarrado = recogible;
    }

    private void Soltar() {
        Destroy(objetoAgarrado.GetComponent<ParentConstraint>());
        objetoAgarrado = null;
    }

}
