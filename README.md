# NIA – Primeros pasos 🎲🌲
### Nia en el Bosque de las Palabras

**Proyecto Final — Programación de Videojuegos II**  
**Autor:** Eduardo Ortega  
**Modalidad:** individual  
**Motor:** Unity · **Lenguaje:** C# · **Plataforma:** WebGL

> Una aventura de acción 3D con cámara superior en la que Nia debe superar enemigos, reunir seis dados con letras y resolver un puzzle de palabras para completar el recorrido.

## 🎮 ¿De qué se trata?

Nia explora un bosque fantástico, enfrenta criaturas con **combate cuerpo a cuerpo (Melee)** y **a distancia (Ranged)** y administra vidas y proyectiles. Cada dado con letra recogido se agrega al inventario y permite recuperar salud. Al reunir los seis dados, una piedra emerge y habilita el desafío final: ordenar las letras en las bases del laberinto. Resolver la palabra activa el grabado a color y la secuencia de victoria.

**Ciclo de juego:** Explorar → combatir → recibir/ocasionar daño → recuperar recursos → recolectar dados → progresar → resolver la palabra → ganar o perder.

## ✨ Características principales

- Mundo **3D Top View / Bird View**, con exploración y combate.
- Ataques **Melee** y **Ranged**; enemigos de proximidad y a distancia.
- Sistemas de daño, salud, derrota y reinicio.
- Inventario de **seis DadosLetra** y sistema de recarga de proyectiles.
- Puzzle final con interacción y formación de palabras.
- Escena de título con **Play, Créditos, Opciones y Exit**.
- Audio **FMOD**, efectos y control de volumen.
- Partida de **aproximadamente 5 a 10 minutos**, según pruebas del autor.

## 🕹️ Controles

| Acción | Entrada |
|---|---|
| Desplazarse | Flechas (según tutorial del juego) |
| Disparar (Ranged) | `F` |
| Interactuar / colocar o retirar dados | `E` |
| Combate Melee | Consultar indicaciones dentro del juego y validar tecla/botón en versión final |

## 🧩 Objetivo

1. Explorar y sobrevivir a los enemigos.
2. Capturar los **6 dados con letras**.
3. Llegar al laberinto cuando se activa el evento de la piedra.
4. Ordenar las letras correctamente en las bases.
5. Completar el puzzle y activar la victoria.

## 🔧 Tecnologías y decisiones de arquitectura

| Recurso | Aplicación |
|---|---|
| **Unity + C#** | Desarrollo del videojuego 3D |
| **FMOD** | Audio ambiental, efectos y controles de volumen |
| **Herencia / polimorfismo** | Base común y variantes de enemigos |
| **Máquina de estados** | Comportamientos y transiciones |
| **Object Pooling** | Reutilización de proyectiles |
| **ScriptableObjects** | Configuración de datos de juego |
| **Eventos / delegates** | Comunicación entre sistemas |
| **JSON** | Catálogo/selección de palabras |

Estos mecanismos fueron confirmados por el autor durante la auditoría funcional. La documentación técnica del proyecto detalla su propósito; los nombres y vínculos exactos deben cotejarse con la revisión final de los scripts.

## 📂 Estructura del repositorio

```text
Assets/           # Escenas, scripts, modelos, sonidos y recursos de Unity
Packages/         # Dependencias del proyecto
ProjectSettings/  # Configuración de Unity
.gitignore        # Exclusiones del control de versiones
```

> Para incorporar la documentación, agregar `README.md` a la raíz y los documentos a `Documentacion/` (carpeta sugerida; no se afirma que exista todavía).

## ▶️ Ejecución

**WebGL:** existe una versión publicada y jugable según el autor.  
**Enlace itch.io de NIA:** `PENDIENTE_DE_AGREGAR_URL_EXACTA`.

**Desde Unity:** clonar este repositorio, abrir la carpeta del proyecto desde Unity Hub con la versión de Unity compatible con `ProjectSettings/ProjectVersion.txt`, esperar la importación y abrir la escena inicial configurada en Build Settings. No se especifica aquí una versión del motor sin verificar el archivo real.

## 🧪 Estado de pruebas

El autor confirmó el funcionamiento de: combate Melee y Ranged, enemigos, vida, derrota, reinicio, recolección, inventario, puzzle, victoria, menú, controles de volumen, audio FMOD y build jugable fuera de Unity.

## 📖 Documentación

El paquete de entrega contiene:
- `01_Game_Concept_NIA.docx`
- `02_High_Concept_NIA.docx`
- `03_One_Page_NIA.docx`
- `04_Informe_Tecnico_Auditoria_NIA.docx`
- `05_Pitch_NIA.pptx`
- `PLAN_PRODUCCION.md`

## 🎨 Créditos y licencias

**Diseño, programación, integración y presentación del proyecto:** Eduardo Ortega.  
Los créditos del juego están implementados. Antes de la entrega formal, completar el inventario y atribución de **recursos externos** (modelos, música, SFX, tipografías y herramientas) con sus fuentes y licencias.

## 🔗 Repositorio

https://github.com/Edudando/Udeci_VJ_PROG2_TP2

---
*Proyecto académico · Programación de Videojuegos II · 2026.*
