const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
  Header, Footer, AlignmentType, HeadingLevel, BorderStyle, WidthType,
  ShadingType, VerticalAlign, PageNumber, PageBreak, LevelFormat, TableOfContents,
  ExternalHyperlink
} = require('docx');
const fs = require('fs');

// ─── CONSTANTES DE COLOR Y FUENTE ────────────────────────────────────────────
const FONT       = "Calibri";
const AZUL       = "1F4E79";
const AZUL_CLARO = "DEEAF1";
const GRIS       = "F2F2F2";
const BORDE      = { style: BorderStyle.SINGLE, size: 4, color: "BFBFBF" };
const BORDES     = { top: BORDE, bottom: BORDE, left: BORDE, right: BORDE };

// ─── HELPERS ──────────────────────────────────────────────────────────────────

const p = (text, opts = {}) => new Paragraph({
  alignment: opts.center ? AlignmentType.CENTER : AlignmentType.JUSTIFIED,
  spacing: { after: opts.spacingAfter ?? 160, before: opts.spacingBefore ?? 0, line: 276 },
  children: [new TextRun({
    text,
    font: FONT,
    size: opts.size ?? 24,
    bold: opts.bold ?? false,
    color: opts.color ?? "000000",
    italics: opts.italic ?? false,
  })]
});

const titulo = (text) => new Paragraph({
  alignment: AlignmentType.CENTER,
  spacing: { after: 200, before: 200 },
  children: [new TextRun({ text, font: FONT, size: 48, bold: true, color: AZUL })]
});

const subtitulo = (text) => new Paragraph({
  alignment: AlignmentType.CENTER,
  spacing: { after: 160, before: 0 },
  children: [new TextRun({ text, font: FONT, size: 28, bold: false, color: "404040" })]
});

const salto = () => new Paragraph({ children: [new PageBreak()] });

const separador = () => new Paragraph({
  spacing: { after: 120, before: 120 },
  border: { bottom: { style: BorderStyle.SINGLE, size: 6, color: "1F4E79", space: 1 } },
  children: []
});

// Celda de tabla
const celda = (text, opts = {}) => new TableCell({
  borders: BORDES,
  width: { size: opts.w ?? 2340, type: WidthType.DXA },
  shading: { fill: opts.fill ?? "FFFFFF", type: ShadingType.CLEAR },
  margins: { top: 80, bottom: 80, left: 120, right: 120 },
  verticalAlign: VerticalAlign.CENTER,
  children: [new Paragraph({
    alignment: opts.center ? AlignmentType.CENTER : AlignmentType.LEFT,
    children: [new TextRun({
      text,
      font: FONT,
      size: 20,
      bold: opts.bold ?? false,
      color: opts.color ?? "000000"
    })]
  })]
});

const fila = (celdas) => new TableRow({ children: celdas });

const bala = (text) => new Paragraph({
  numbering: { reference: "bullets", level: 0 },
  spacing: { after: 80 },
  children: [new TextRun({ text, font: FONT, size: 24 })]
});

const balaNum = (text) => new Paragraph({
  numbering: { reference: "numbers", level: 0 },
  spacing: { after: 80 },
  children: [new TextRun({ text, font: FONT, size: 24 })]
});

const codigo = (text) => new Paragraph({
  spacing: { after: 60, before: 60 },
  indent: { left: 720 },
  children: [new TextRun({ text, font: "Courier New", size: 18, color: "2B2B2B" })]
});

// ─── SECCIONES ────────────────────────────────────────────────────────────────

// ── PORTADA ──
const portada = [
  new Paragraph({ spacing: { after: 2200 }, children: [] }),
  titulo("RelaxingFarm"),
  subtitulo("Videojuego Idle Clicker con Autenticación y Backend en Firebase"),
  separador(),
  new Paragraph({ spacing: { after: 800 }, children: [] }),
  p("Alumno/a:   [TU NOMBRE COMPLETO]",  { center: true, size: 24, spacingAfter: 120 }),
  p("Tutor/a:    [NOMBRE DEL TUTOR/A]",  { center: true, size: 24, spacingAfter: 120 }),
  p("Centro:     [NOMBRE DEL CENTRO]",   { center: true, size: 24, spacingAfter: 120 }),
  p("Ciclo:      Desarrollo de Aplicaciones Multiplataforma (DAM)", { center: true, size: 24, spacingAfter: 120 }),
  p("Año académico:   2025 – 2026",      { center: true, size: 24, spacingAfter: 300 }),
  salto(),
];

// ── ÍNDICE ──
const indice = [
  new Paragraph({
    heading: HeadingLevel.HEADING_1,
    children: [new TextRun({ text: "Índice", font: FONT, size: 32, bold: true, color: AZUL })]
  }),
  new TableOfContents("Índice de contenidos", { hyperlink: true, headingStyleRange: "1-3" }),
  salto(),
];

// ── 1. INTRODUCCIÓN ──
const introduccion = [
  new Paragraph({
    heading: HeadingLevel.HEADING_1,
    children: [new TextRun({ text: "1.  Introducción", font: FONT, size: 32, bold: true, color: AZUL })]
  }),
  separador(),
  new Paragraph({
    heading: HeadingLevel.HEADING_2,
    children: [new TextRun({ text: "1.1  Contexto y motivación personal", font: FONT, size: 26, bold: true, color: "2E4057" })]
  }),
  p("[SUGERENCIA — Explica en 3-4 párrafos por qué elegiste hacer un videojuego como proyecto de fin de ciclo, tu experiencia previa con videojuegos o programación, y por qué el género idle clicker te resultó interesante para demostrar las competencias del ciclo: persistencia de datos, servicios en la nube, arquitectura de software, etc.]", { italic: true, color: "888888" }),
  p("En los últimos años, los videojuegos de tipo idle clicker han experimentado un notable crecimiento en plataformas de escritorio y móviles, gracias a su mecánica accesible y su capacidad de generar satisfacción a través del progreso acumulativo. Partiendo de ese género, este proyecto desarrolla RelaxingFarm, un videojuego de temática rural y relajante orientado a un público amplio."),
  new Paragraph({
    heading: HeadingLevel.HEADING_2,
    children: [new TextRun({ text: "1.2  Descripción general de la aplicación", font: FONT, size: 26, bold: true, color: "2E4057" })]
  }),
  p("RelaxingFarm es un videojuego 2D desarrollado con el motor Unity en el que el jugador gestiona una granja virtual compuesta por parcelas de cultivo. Cada parcela genera dinero de forma automática al completar ciclos de producción. Con ese dinero, el jugador puede desbloquear nuevas parcelas, adquirir mejoras permanentes y competir con otros jugadores en un ranking global."),
  p("La aplicación se estructura en dos escenas principales:"),
  bala("LoginScene: pantalla de registro e inicio de sesión mediante correo electrónico y contraseña, gestionada a través de la Identity Platform de Firebase mediante REST API."),
  bala("MainGame: escena de juego principal con la granja, la tienda, el panel de mejoras, el perfil de usuario, el ranking global y la pantalla de configuración."),
  p("Toda la información del jugador —dinero, parcelas desbloqueadas, mejoras activas, nombre de usuario y estadísticas— se sincroniza automáticamente con Firebase Realtime Database, permitiendo continuar la partida desde cualquier dispositivo con la misma cuenta."),
  p("[SUGERENCIA — Inserta aquí una captura de pantalla de la pantalla principal del juego y otra de la pantalla de login.]", { italic: true, color: "888888" }),
  salto(),
];

// ── 2. OBJETIVOS ──
const objetivos = [
  new Paragraph({
    heading: HeadingLevel.HEADING_1,
    children: [new TextRun({ text: "2.  Objetivos", font: FONT, size: 32, bold: true, color: AZUL })]
  }),
  separador(),
  new Paragraph({
    heading: HeadingLevel.HEADING_2,
    children: [new TextRun({ text: "2.1  Objetivos funcionales", font: FONT, size: 26, bold: true, color: "2E4057" })]
  }),
  p("Los objetivos funcionales definen las características que el usuario final puede percibir y utilizar:"),
  new Table({
    width: { size: 9360, type: WidthType.DXA },
    columnWidths: [1200, 8160],
    rows: [
      fila([celda("ID", { w: 1200, bold: true, fill: AZUL_CLARO, center: true }), celda("Descripción", { w: 8160, bold: true, fill: AZUL_CLARO })]),
      fila([celda("OF-01", { w: 1200, center: true }), celda("El sistema permitirá registrar una cuenta nueva con correo electrónico y contraseña.", { w: 8160 })]),
      fila([celda("OF-02", { w: 1200, center: true }), celda("El sistema permitirá iniciar y cerrar sesión de forma segura.", { w: 8160 })]),
      fila([celda("OF-03", { w: 1200, center: true }), celda("El jugador podrá generar dinero de forma pasiva mediante parcelas de producción con distintos ciclos y ratios.", { w: 8160 })]),
      fila([celda("OF-04", { w: 1200, center: true }), celda("El jugador podrá generar dinero activo pulsando el botón de la regadera.", { w: 8160 })]),
      fila([celda("OF-05", { w: 1200, center: true }), celda("El jugador podrá comprar nuevas parcelas en la tienda si dispone de fondos suficientes.", { w: 8160 })]),
      fila([celda("OF-06", { w: 1200, center: true }), celda("El jugador podrá adquirir mejoras permanentes que aumenten la velocidad de producción, el dinero por ciclo o el poder de clic.", { w: 8160 })]),
      fila([celda("OF-07", { w: 1200, center: true }), celda("El jugador podrá consultar y editar su nombre de usuario en la pantalla de perfil.", { w: 8160 })]),
      fila([celda("OF-08", { w: 1200, center: true }), celda("El jugador podrá reiniciar su progreso completo, previa confirmación.", { w: 8160 })]),
      fila([celda("OF-09", { w: 1200, center: true }), celda("El jugador podrá eliminar su cuenta y todos sus datos, previa confirmación.", { w: 8160 })]),
      fila([celda("OF-10", { w: 1200, center: true }), celda("El jugador podrá ver un ranking global con los 10 mejores jugadores ordenados por dinero total acumulado.", { w: 8160 })]),
      fila([celda("OF-11", { w: 1200, center: true }), celda("La partida se guardará automáticamente en la nube cada dos minutos y al cerrar la aplicación.", { w: 8160 })]),
      fila([celda("OF-12", { w: 1200, center: true }), celda("La aplicación reproducirá música de fondo con volumen configurable mediante un slider.", { w: 8160 })]),
      fila([celda("OF-13", { w: 1200, center: true }), celda("La aplicación mostrará un aviso visible si se produce un error de conectividad con Firebase.", { w: 8160 })]),
    ]
  }),
  new Paragraph({ spacing: { after: 200 }, children: [] }),
  new Paragraph({
    heading: HeadingLevel.HEADING_2,
    children: [new TextRun({ text: "2.2  Objetivos técnicos", font: FONT, size: 26, bold: true, color: "2E4057" })]
  }),
  p("Los objetivos técnicos establecen las decisiones de diseño e implementación que se deben alcanzar:"),
  new Table({
    width: { size: 9360, type: WidthType.DXA },
    columnWidths: [1200, 8160],
    rows: [
      fila([celda("ID", { w: 1200, bold: true, fill: AZUL_CLARO, center: true }), celda("Descripción", { w: 8160, bold: true, fill: AZUL_CLARO })]),
      fila([celda("OT-01", { w: 1200, center: true }), celda("Utilizar Unity 2D como motor de desarrollo para obtener compatibilidad multiplataforma sin herramientas adicionales.", { w: 8160 })]),
      fila([celda("OT-02", { w: 1200, center: true }), celda("Implementar la comunicación con Firebase exclusivamente mediante la REST API, sin dependencia del SDK oficial de Firebase para Unity.", { w: 8160 })]),
      fila([celda("OT-03", { w: 1200, center: true }), celda("Aplicar el patrón Singleton con DontDestroyOnLoad para que los gestores principales persistan entre escenas.", { w: 8160 })]),
      fila([celda("OT-04", { w: 1200, center: true }), celda("Gestionar la expiración del token de Firebase (60 min) mediante un refresco automático cada 50 minutos.", { w: 8160 })]),
      fila([celda("OT-05", { w: 1200, center: true }), celda("Serializar y deserializar datos JSON de forma manual para controlar el formato y evitar conflictos con la configuración regional del dispositivo (coma vs. punto decimal).", { w: 8160 })]),
      fila([celda("OT-06", { w: 1200, center: true }), celda("Separar la lógica de cada pantalla en scripts independientes (Manager pattern) para facilitar el mantenimiento.", { w: 8160 })]),
      fila([celda("OT-07", { w: 1200, center: true }), celda("Documentar todos los scripts con comentarios XML para que el código sea legible y mantenible.", { w: 8160 })]),
      fila([celda("OT-08", { w: 1200, center: true }), celda("Producir un ejecutable para Windows (.exe) listo para distribución.", { w: 8160 })]),
    ]
  }),
  new Paragraph({ spacing: { after: 200 }, children: [] }),
  salto(),
];

// ── 3.1 FASE DE DISEÑO ──
const faseDiseño = [
  new Paragraph({
    heading: HeadingLevel.HEADING_1,
    children: [new TextRun({ text: "3.  Fases del proyecto", font: FONT, size: 32, bold: true, color: AZUL })]
  }),
  separador(),
  new Paragraph({
    heading: HeadingLevel.HEADING_2,
    children: [new TextRun({ text: "3.1  Fase de diseño", font: FONT, size: 26, bold: true, color: "2E4057" })]
  }),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.1.1  Elección de tecnologías", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  new Table({
    width: { size: 9360, type: WidthType.DXA },
    columnWidths: [2200, 7160],
    rows: [
      fila([celda("Tecnología", { w: 2200, bold: true, fill: AZUL_CLARO }), celda("Justificación", { w: 7160, bold: true, fill: AZUL_CLARO })]),
      fila([celda("Unity 2022 LTS", { w: 2200, bold: true }), celda("Motor de juego multiplataforma con amplia comunidad, documentación en español, soporte nativo 2D, sistema UI integrado y exportación a Windows, Android e iOS.", { w: 7160 })]),
      fila([celda("C#", { w: 2200, bold: true }), celda("Lenguaje principal de Unity. Fuertemente tipado, orientado a objetos, con soporte de Coroutines para operaciones asíncronas.", { w: 7160 })]),
      fila([celda("Firebase (Google)", { w: 2200, bold: true }), celda("Base de datos en tiempo real, autenticación por correo/contraseña, REST API sencilla sin SDK adicional, plan gratuito Spark suficiente para el proyecto.", { w: 7160 })]),
      fila([celda("TextMeshPro", { w: 2200, bold: true }), celda("Renderizado de texto de alta calidad (SDF) compatible con cualquier resolución, incluido en Unity via Package Manager.", { w: 7160 })]),
    ]
  }),
  new Paragraph({ spacing: { after: 200 }, children: [] }),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.1.2  Arquitectura del sistema", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("El sistema se organiza en tres capas claramente diferenciadas:"),
  bala("Capa de presentación (Unity): contiene las dos escenas del juego (LoginScene y MainGame) con todos los scripts de interfaz de usuario (LoginManager, GameManager, TiendaManager, MejorasManager, PerfilManager, ConfiguracionManager, RankingManager, Parcela, CircleButton, BtnRegadera)."),
  bala("Capa de comunicación (SaveManager + FirebaseManager): gestiona las peticiones HTTP (GET, PUT, PATCH, DELETE) mediante UnityWebRequest, la serialización JSON manual con CultureInfo.InvariantCulture y la renovación del token de autenticación."),
  bala("Capa de datos (Firebase Cloud): almacena las cuentas de usuario en Firebase Authentication y los datos del juego en Firebase Realtime Database bajo los nodos /usuarios, /partidas, /rankings y /logs."),
  p("Patrón de persistencia entre escenas: GameManager, SaveManager y FirebaseManager se instancian en LoginScene y se marcan con DontDestroyOnLoad. Al cargar MainGame, estos objetos permanecen activos y la escena de juego conecta sus referencias de UI mediante GameObject.Find() en el método CargarTodo()."),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.1.3  Estructura de la base de datos Firebase", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("El nodo raíz de Firebase Realtime Database se organiza de la siguiente forma:"),
  new Table({
    width: { size: 9360, type: WidthType.DXA },
    columnWidths: [3000, 1800, 4560],
    rows: [
      fila([celda("Nodo", { w: 3000, bold: true, fill: AZUL_CLARO }), celda("Tipo", { w: 1800, bold: true, fill: AZUL_CLARO }), celda("Descripción", { w: 4560, bold: true, fill: AZUL_CLARO })]),
      fila([celda("/usuarios/{uid}/nombre", { w: 3000 }), celda("string", { w: 1800 }), celda("Nombre de usuario visible", { w: 4560 })]),
      fila([celda("/usuarios/{uid}/email", { w: 3000 }), celda("string", { w: 1800 }), celda("Correo electrónico", { w: 4560 })]),
      fila([celda("/partidas/{uid}/dinero", { w: 3000 }), celda("number", { w: 1800 }), celda("Monedas disponibles actualmente", { w: 4560 })]),
      fila([celda("/partidas/{uid}/dineroTotal", { w: 3000 }), celda("number", { w: 1800 }), celda("Total acumulado histórico", { w: 4560 })]),
      fila([celda("/partidas/{uid}/parcelas/producto_N", { w: 3000 }), celda("object", { w: 1800 }), celda("Estado de cada parcela (9 parcelas, índice 0-8)", { w: 4560 })]),
      fila([celda("/rankings/{uid}/nombreUsuario", { w: 3000 }), celda("string", { w: 1800 }), celda("Nombre visible en el ranking", { w: 4560 })]),
      fila([celda("/rankings/{uid}/dineroTotal", { w: 3000 }), celda("number", { w: 1800 }), celda("Total acumulado para ordenar", { w: 4560 })]),
      fila([celda("/rankings/{uid}/nivel", { w: 3000 }), celda("number", { w: 1800 }), celda("Número de parcelas compradas", { w: 4560 })]),
      fila([celda("/logs/{uid}/ultimoAcceso", { w: 3000 }), celda("string", { w: 1800 }), celda("Timestamp ISO 8601 del último guardado", { w: 4560 })]),
    ]
  }),
  new Paragraph({ spacing: { after: 200 }, children: [] }),
  p("[SUGERENCIA — Incluye aquí una captura de la consola de Firebase mostrando la estructura real con datos de ejemplo, y otra captura de las Security Rules configuradas.]", { italic: true, color: "888888" }),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.1.4  Diseño de pantallas (wireframes)", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("[SUGERENCIA — Inserta aquí los bocetos o capturas de cada pantalla con una breve descripción: Login/Registro, Juego principal, Tienda, Mejoras, Perfil, Configuración y Ranking. Puedes usar capturas reales de Unity o bocetos en papel escaneados.]", { italic: true, color: "888888" }),
];

// ── 3.2 FASE DE IMPLEMENTACIÓN ──
const faseImplementacion = [
  new Paragraph({
    heading: HeadingLevel.HEADING_2,
    children: [new TextRun({ text: "3.2  Fase de implementación", font: FONT, size: 26, bold: true, color: "2E4057" })]
  }),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.2.1  Configuración del entorno de desarrollo", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  bala("Unity Hub 3.x + Unity Editor 2022.3 LTS (plantilla 2D)"),
  bala("Visual Studio 2022 Community (editor de código integrado con Unity)"),
  bala("TextMeshPro importado via Window > Package Manager"),
  bala("Firebase Console (consola web de Google) para configuración del backend"),
  bala("Git para control de versiones local"),
  p("[SUGERENCIA — Menciona el sistema operativo y las especificaciones del equipo de desarrollo, y cualquier herramienta adicional usada para los assets visuales (Aseprite, Photoshop, etc.).]", { italic: true, color: "888888" }),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.2.2  Estructura del proyecto en Unity", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("Los 12 scripts del proyecto se distribuyen en las dos escenas de la siguiente manera:"),
  bala("LoginScene: contiene el GameObject Managers (DontDestroyOnLoad) con los scripts GameManager, SaveManager y FirebaseManager, más el Canvas de login con LoginManager."),
  bala("MainGame: contiene el Canvas principal con los 9 GameObjects de tipo Parcela, el botón de regadera (CircleButton + BtnRegadera), los paneles de navegación (TiendaManager, MejorasManager, PerfilManager, ConfiguracionManager, RankingManager) y los textos de UI (dinero, producción/s, banner de error)."),
  p("El AudioListener se ubica en el GameObject del GameManager (DontDestroyOnLoad) para evitar que el audio se interrumpa durante la transición entre escenas. Las cámaras de ambas escenas tienen el AudioListener desactivado."),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.2.3  FirebaseManager — Sistema de autenticación", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("FirebaseManager centraliza toda la comunicación con Firebase Authentication a través de la Identity Platform REST API. Gestiona el registro de nuevas cuentas (POST a /accounts:signUp), el inicio de sesión (POST a /accounts:signInWithPassword) y el refresco automático del token de autenticación (POST a /token con grant_type=refresh_token)."),
  p("Los tokens de Firebase caducan a los 60 minutos. Para evitar fallos silenciosos en sesiones largas, SaveManager invoca RefrescarToken cada 50 minutos mediante InvokeRepeating."),
  p("Decisión de diseño: no se usa el SDK oficial de Firebase para Unity porque añade dependencias de varios megabytes y complica la configuración en distintas plataformas. La REST API ofrece exactamente las mismas operaciones para el alcance de este proyecto."),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.2.4  GameManager — Núcleo del juego", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("GameManager es el Singleton principal que persiste entre escenas. Almacena el estado global del jugador: dinero disponible, dinero total acumulado, clickPower, prestigeMultiplicador, nivel, vecesPrestige, parcelasCompradasTotal y el array de 9 referencias a los scripts Parcela."),
  p("El método AñadirDinero() multiplica la cantidad por prestigeMultiplicador antes de sumarla, lo que permite que el sistema de prestige escale la producción sin modificar los valores individuales de las parcelas. ActualizarUI() refresca los textos de dinero y producción por segundo en la barra superior. MostrarErrorRed() activa un banner rojo durante 3 segundos cuando se produce un error de conectividad, con un flag antiSpam para evitar superposiciones."),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.2.5  Parcela — Unidad de producción", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("Cada una de las 9 parcelas es un MonoBehaviour independiente que gestiona su propio ciclo de producción. En cada frame, si la parcela está desbloqueada, incrementa su timerActual con Time.deltaTime y actualiza el fillAmount de la barraProgreso para mostrar el avance visual. Al completar el ciclo (timerActual >= tiempoCiclo), reinicia el timer y llama a GameManager.Instance.AñadirDinero(dineroPerCiclo)."),
  p("La tabla siguiente muestra los valores base de los 9 productos disponibles:"),
  new Table({
    width: { size: 9360, type: WidthType.DXA },
    columnWidths: [500, 2860, 1600, 2000, 2400],
    rows: [
      fila([
        celda("#",           { w: 500,  bold: true, fill: AZUL_CLARO, center: true }),
        celda("Producto",    { w: 2860, bold: true, fill: AZUL_CLARO }),
        celda("Coste ($)",   { w: 1600, bold: true, fill: AZUL_CLARO, center: true }),
        celda("$/ciclo",     { w: 2000, bold: true, fill: AZUL_CLARO, center: true }),
        celda("Ciclo (s)",   { w: 2400, bold: true, fill: AZUL_CLARO, center: true }),
      ]),
      fila([celda("0", { w: 500, center: true }), celda("Semilla de Girasol",  { w: 2860 }), celda("Gratis", { w: 1600, center: true }), celda("5",     { w: 2000, center: true }), celda("2",   { w: 2400, center: true })]),
      fila([celda("1", { w: 500, center: true }), celda("Zanahorias",          { w: 2860 }), celda("50",     { w: 1600, center: true }), celda("10",    { w: 2000, center: true }), celda("5",   { w: 2400, center: true })]),
      fila([celda("2", { w: 500, center: true }), celda("Tomates",             { w: 2860 }), celda("100",    { w: 1600, center: true }), celda("20",    { w: 2000, center: true }), celda("10",  { w: 2400, center: true })]),
      fila([celda("3", { w: 500, center: true }), celda("Maíz",                { w: 2860 }), celda("200",    { w: 1600, center: true }), celda("35",    { w: 2000, center: true }), celda("15",  { w: 2400, center: true })]),
      fila([celda("4", { w: 500, center: true }), celda("Berenjenas",          { w: 2860 }), celda("500",    { w: 1600, center: true }), celda("60",    { w: 2000, center: true }), celda("20",  { w: 2400, center: true })]),
      fila([celda("5", { w: 500, center: true }), celda("Girasoles",           { w: 2860 }), celda("1.000",  { w: 1600, center: true }), celda("100",   { w: 2000, center: true }), celda("30",  { w: 2400, center: true })]),
      fila([celda("6", { w: 500, center: true }), celda("Granja de Gallinas",  { w: 2860 }), celda("2.000",  { w: 1600, center: true }), celda("200",   { w: 2000, center: true }), celda("60",  { w: 2400, center: true })]),
      fila([celda("7", { w: 500, center: true }), celda("Granja de Ovejas",    { w: 2860 }), celda("5.000",  { w: 1600, center: true }), celda("500",   { w: 2000, center: true }), celda("120", { w: 2400, center: true })]),
      fila([celda("8", { w: 500, center: true }), celda("Granja de Vacas",     { w: 2860 }), celda("10.000", { w: 1600, center: true }), celda("1.000", { w: 2000, center: true }), celda("240", { w: 2400, center: true })]),
    ]
  }),
  new Paragraph({ spacing: { after: 200 }, children: [] }),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.2.6  SaveManager — Sistema de guardado", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("SaveManager gestiona toda la persistencia en la nube mediante la REST API de Firebase Realtime Database. Su método principal, CargarTodo(), se ejecuta tras el login: descarga los datos del usuario (/usuarios/{uid}) y de la partida (/partidas/{uid}), carga la escena MainGame y usa GameObject.Find() para cablear las referencias de UI al GameManager, que al ser DontDestroyOnLoad no puede tener esas referencias cableadas desde el Inspector de MainGame."),
  p("El guardado construye el JSON manualmente con StringBuilder usando CultureInfo.InvariantCulture para todos los valores numéricos, evitando que la coma decimal del español corrompa los datos en Firebase. Se guarda automáticamente cada 2 minutos (InvokeRepeating) y también al cerrar o pausar la aplicación (OnApplicationQuit, OnApplicationPause)."),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.2.7  TiendaManager — Tienda de parcelas", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("La tienda muestra los 9 productos con su coste, producción por ciclo y estado. Usa un código de colores para guiar al usuario: verde si la parcela ya está comprada, dorado si hay fondos suficientes para comprarla, y negro oscuro si no hay fondos. El método ComprarParcela() descuenta el coste, asigna los valores de producción, desbloquea la parcela y aplica retroactivamente las mejoras ya adquiridas mediante MejorasManager.AplicarMejorasAParcelaNueva()."),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.2.8  MejorasManager — Sistema de mejoras", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("Las mejoras son compras únicas con efecto permanente que modifican los valores de producción de todas las parcelas desbloqueadas. AplicarMejorasAParcelaNueva() garantiza que las parcelas compradas después de adquirir una mejora también reciben ese beneficio. ResetearMejoras() pone todos los flags a false al reiniciar el progreso."),
  p("[SUGERENCIA — Lista aquí las mejoras concretas que implementaste con su coste y efecto, por ejemplo: Riego Rápido (-20% ciclo), Abono Premium (+50% $/ciclo), Clic Turbo (x2 clickPower), etc.]", { italic: true, color: "888888" }),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.2.9  PerfilManager — Perfil de usuario", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("Muestra las estadísticas del jugador (nombre, email, dinero total, nivel, veces prestige) y permite editar el nombre de usuario mediante un subpanel emergente con TMP_InputField. Al guardar, el nuevo nombre se persiste en PlayerPrefs y se sincroniza en Firebase con dos peticiones PATCH: una a /usuarios/{uid} y otra a /rankings/{uid} para mantener la consistencia en el ranking global."),
  p("ReiniciarProgreso() resetea todos los valores del GameManager, bloquea las 9 parcelas, elimina los efectos de las mejoras mediante ResetearParcelasAValoresBase() y desbloquea la parcela 0 de forma gratuita. EliminarCuenta() borra secuencialmente los cuatro nodos del usuario en Firebase, elimina la cuenta de Authentication y carga la escena de login."),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.2.10  ConfiguracionManager — Pantalla de ajustes", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("Controla el volumen maestro del juego mediante AudioListener.volume y un Slider de Unity UI. El valor se persiste en PlayerPrefs. Un panel de confirmación reutilizable (con un flag booleano esperandoEliminar) sirve tanto para la acción Reiniciar Progreso como para Eliminar Cuenta, evitando duplicar componentes de UI. SalirJuego() llama a Application.Quit() para cerrar la aplicación en el build de PC."),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.2.11  RankingManager — Ranking global", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("Descarga el nodo /rankings.json completo de Firebase y parsea manualmente el JSON resultante (estructura { \"uid1\": {...}, \"uid2\": {...} }) buscando bloques \":{\" con un parser ad hoc que usa búsqueda de subcadenas y CultureInfo.InvariantCulture para los valores numéricos. Las entradas se ordenan por dineroTotal descendente y se muestran las 10 primeras."),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.2.12  LoginManager — Pantalla de acceso", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("Gestiona los formularios de registro e inicio de sesión. Durante el procesamiento muestra un panel de carga semitransparente (\"Cargando...\") que bloquea la interacción, evitando que el usuario pulse varias veces el botón. Tras recibir la respuesta de Firebase, oculta el panel y muestra el mensaje de error si procede."),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.2.13  CircleButton y BtnRegadera — Controles", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("CircleButton sobreescribe IsRaycastLocationValid() para restringir el área de clic a una forma circular, ignorando las esquinas transparentes del sprite rectangular. BtnRegadera gestiona el clic manual: al pulsar, llama a GameManager.AñadirDinero(dineroPerClick * clickPower), donde dineroPerClick es un valor configurable desde el Inspector."),
  salto(),
];

// ── 3.3 FASE DE PRUEBAS ──
const fasePruebas = [
  new Paragraph({
    heading: HeadingLevel.HEADING_2,
    children: [new TextRun({ text: "3.3  Fase de pruebas", font: FONT, size: 26, bold: true, color: "2E4057" })]
  }),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.3.1  Pruebas funcionales", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("A continuación se recogen los casos de prueba realizados sobre la aplicación:"),
  new Table({
    width: { size: 9360, type: WidthType.DXA },
    columnWidths: [900, 5260, 2200, 1000],
    rows: [
      fila([
        celda("ID",          { w: 900,  bold: true, fill: AZUL_CLARO, center: true }),
        celda("Descripción", { w: 5260, bold: true, fill: AZUL_CLARO }),
        celda("Resultado esperado", { w: 2200, bold: true, fill: AZUL_CLARO }),
        celda("Estado",      { w: 1000, bold: true, fill: AZUL_CLARO, center: true }),
      ]),
      fila([celda("P01", { w: 900, center: true }), celda("Registro con email válido y contraseña",        { w: 5260 }), celda("Cuenta creada y login automático",   { w: 2200 }), celda("OK", { w: 1000, center: true, fill: "#CCFFCC" })]),
      fila([celda("P02", { w: 900, center: true }), celda("Registro con email ya existente",               { w: 5260 }), celda("Mensaje de error visible",          { w: 2200 }), celda("OK", { w: 1000, center: true, fill: "#CCFFCC" })]),
      fila([celda("P03", { w: 900, center: true }), celda("Login con credenciales correctas",              { w: 5260 }), celda("Carga MainGame con progreso",        { w: 2200 }), celda("OK", { w: 1000, center: true, fill: "#CCFFCC" })]),
      fila([celda("P04", { w: 900, center: true }), celda("Login con contraseña incorrecta",               { w: 5260 }), celda("Error de autenticación visible",     { w: 2200 }), celda("OK", { w: 1000, center: true, fill: "#CCFFCC" })]),
      fila([celda("P05", { w: 900, center: true }), celda("Comprar parcela con fondos suficientes",        { w: 5260 }), celda("Parcela activa y produciendo",       { w: 2200 }), celda("OK", { w: 1000, center: true, fill: "#CCFFCC" })]),
      fila([celda("P06", { w: 900, center: true }), celda("Intentar comprar parcela sin fondos",           { w: 5260 }), celda("Sin efecto, botón bloqueado",        { w: 2200 }), celda("OK", { w: 1000, center: true, fill: "#CCFFCC" })]),
      fila([celda("P07", { w: 900, center: true }), celda("Comprar mejora y verificar efecto en parcelas", { w: 5260 }), celda("Producción aumentada correctamente", { w: 2200 }), celda("OK", { w: 1000, center: true, fill: "#CCFFCC" })]),
      fila([celda("P08", { w: 900, center: true }), celda("Cerrar y reabrir la aplicación",                { w: 5260 }), celda("Progreso recuperado íntegramente",   { w: 2200 }), celda("OK", { w: 1000, center: true, fill: "#CCFFCC" })]),
      fila([celda("P09", { w: 900, center: true }), celda("Editar nombre de usuario",                      { w: 5260 }), celda("Nombre actualizado en perfil y ranking", { w: 2200 }), celda("OK", { w: 1000, center: true, fill: "#CCFFCC" })]),
      fila([celda("P10", { w: 900, center: true }), celda("Abrir el ranking global",                       { w: 5260 }), celda("Top 10 visible con datos correctos", { w: 2200 }), celda("OK", { w: 1000, center: true, fill: "#CCFFCC" })]),
      fila([celda("P11", { w: 900, center: true }), celda("Reiniciar progreso con confirmación",           { w: 5260 }), celda("Todo vuelve a cero, parcela 0 activa", { w: 2200 }), celda("OK", { w: 1000, center: true, fill: "#CCFFCC" })]),
      fila([celda("P12", { w: 900, center: true }), celda("Eliminar cuenta con confirmación",              { w: 5260 }), celda("Datos borrados y vuelta al login",   { w: 2200 }), celda("OK", { w: 1000, center: true, fill: "#CCFFCC" })]),
      fila([celda("P13", { w: 900, center: true }), celda("Mover el slider de volumen",                   { w: 5260 }), celda("Volumen de la música cambia en tiempo real", { w: 2200 }), celda("OK", { w: 1000, center: true, fill: "#CCFFCC" })]),
      fila([celda("P14", { w: 900, center: true }), celda("Botón Salir en la build de PC",                 { w: 5260 }), celda("La aplicación se cierra",            { w: 2200 }), celda("OK", { w: 1000, center: true, fill: "#CCFFCC" })]),
      fila([celda("P15", { w: 900, center: true }), celda("Sesión activa durante más de 1 hora",           { w: 5260 }), celda("Token refrescado automáticamente, guardado sin errores", { w: 2200 }), celda("OK", { w: 1000, center: true, fill: "#CCFFCC" })]),
    ]
  }),
  new Paragraph({ spacing: { after: 200 }, children: [] }),
  new Paragraph({
    heading: HeadingLevel.HEADING_3,
    children: [new TextRun({ text: "3.3.2  Errores detectados y soluciones aplicadas", font: FONT, size: 24, bold: true, color: "4472C4" })]
  }),
  p("A continuación se documentan los bugs más relevantes encontrados durante el desarrollo y las soluciones implementadas:"),
  new Table({
    width: { size: 9360, type: WidthType.DXA },
    columnWidths: [2000, 3680, 3680],
    rows: [
      fila([
        celda("Error",    { w: 2000, bold: true, fill: AZUL_CLARO }),
        celda("Causa",    { w: 3680, bold: true, fill: AZUL_CLARO }),
        celda("Solución", { w: 3680, bold: true, fill: AZUL_CLARO }),
      ]),
      fila([
        celda("Slider de volumen sin efecto", { w: 2000 }),
        celda("El evento OnValueChanged del Slider estaba cableado con 'Static Parameters' en lugar de 'Dynamic float', por lo que siempre pasaba 0 a CambiarVolumen().", { w: 3680 }),
        celda("Cambiar el callback a 'Dynamic float' en el Inspector de Unity para que el Slider pase su valor real en tiempo real.", { w: 3680 }),
      ]),
      fila([
        celda("Música cortada al hacer login", { w: 2000 }),
        celda("El AudioListener estaba en la cámara de LoginScene. Al cargar MainGame, esa cámara se destruía junto con el listener, cortando el audio.", { w: 3680 }),
        celda("Mover el AudioListener al GameObject del GameManager (DontDestroyOnLoad) y desactivar el listener en las cámaras de ambas escenas.", { w: 3680 }),
      ]),
      fila([
        celda("dineroTotal = 0 en el ranking", { w: 2000 }),
        celda("double.Parse() sin CultureInfo.InvariantCulture devuelve 0 silenciosamente en dispositivos con locale español ('1234.5' es inválido con coma decimal). El ToString() al guardar también producía '1234,5'.", { w: 3680 }),
        celda("Usar CultureInfo.InvariantCulture en todos los Parse y ToString de valores double/float del proyecto.", { w: 3680 }),
      ]),
      fila([
        celda("NullReferenceException al cerrar desde login", { w: 2000 }),
        celda("OnApplicationQuit() accedía a FirebaseManager.Instance.uid sin comprobar si la instancia era null (el usuario no había iniciado sesión todavía).", { w: 3680 }),
        celda("Añadir comprobación: if (FirebaseManager.Instance != null && !string.IsNullOrEmpty(...uid)) antes de ejecutar el guardado.", { w: 3680 }),
      ]),
    ]
  }),
  new Paragraph({ spacing: { after: 200 }, children: [] }),
  salto(),
];

// ── 4. AMPLIACIONES ──
const ampliaciones = [
  new Paragraph({
    heading: HeadingLevel.HEADING_1,
    children: [new TextRun({ text: "4.  Ampliación y posibles mejoras", font: FONT, size: 32, bold: true, color: AZUL })]
  }),
  separador(),
  new Paragraph({
    heading: HeadingLevel.HEADING_2,
    children: [new TextRun({ text: "4.1  Mejoras técnicas", font: FONT, size: 26, bold: true, color: "2E4057" })]
  }),
  bala("Migración a Firebase SDK oficial con Firestore, que permite consultas más complejas (filtros, paginación del ranking, búsqueda por nombre)."),
  bala("Implementación de notificaciones push (Firebase Cloud Messaging) para avisar al jugador cuando las parcelas han completado varios ciclos con la app cerrada."),
  bala("Sistema de caché local offline: guardar la partida también en PlayerPrefs para jugar sin conexión y sincronizar al reconectarse."),
  bala("Tests unitarios con Unity Test Framework para verificar las funciones de cálculo de producción y el parseo JSON."),
  bala("Compresión del JSON de guardado para reducir el tráfico en conexiones lentas."),
  new Paragraph({
    heading: HeadingLevel.HEADING_2,
    children: [new TextRun({ text: "4.2  Mejoras de jugabilidad", font: FONT, size: 26, bold: true, color: "2E4057" })]
  }),
  bala("Sistema de Prestige completo: reinicio voluntario con bonificación permanente que aporte rejugabilidad a largo plazo."),
  bala("Animaciones en las parcelas al completar un ciclo (partículas, efectos de brillo, feedback visual)."),
  bala("Logros y metas desbloqueables (primera compra, 1M de dinero acumulado, etc.) con recompensas."),
  bala("Eventos temporales gestionados desde Firebase Remote Config (fin de semana de doble producción, cosecha especial)."),
  bala("Tabla de líderes semanal y mensual separada del ranking global permanente."),
  new Paragraph({
    heading: HeadingLevel.HEADING_2,
    children: [new TextRun({ text: "4.3  Ampliación de plataformas", font: FONT, size: 26, bold: true, color: "2E4057" })]
  }),
  bala("Build para Android con firma APK y publicación en Google Play."),
  bala("Build para iOS con Xcode y certificado de desarrollador Apple."),
  bala("Versión WebGL jugable en el navegador, alojada en Firebase Hosting."),
  salto(),
];

// ── 5. CONCLUSIÓN ──
const conclusion = [
  new Paragraph({
    heading: HeadingLevel.HEADING_1,
    children: [new TextRun({ text: "5.  Conclusión", font: FONT, size: 32, bold: true, color: AZUL })]
  }),
  separador(),
  p("[SUGERENCIA — Escribe 4-6 párrafos en primera persona respondiendo a estas preguntas:]", { italic: true, color: "888888" }),
  p("[Párrafo 1 — Resumen: ¿Qué has construido? ¿Cumple los objetivos planteados al inicio? Ejemplo: \"A lo largo de este proyecto he desarrollado una aplicación completa de tipo videojuego idle clicker, integrando Unity 2D con Firebase para conseguir una experiencia con persistencia de datos en la nube funcional y robusta...\"]", { italic: true, color: "888888" }),
  p("[Párrafo 2 — Dificultades: ¿Cuál fue el reto técnico más complicado? (el parseo JSON manual, la gestión del token, el DontDestroyOnLoad, el bug de CultureInfo...). Explica cómo lo superaste.]", { italic: true, color: "888888" }),
  p("[Párrafo 3 — Conocimientos adquiridos: ¿Qué has aprendido que no sabías antes? (Unity, Firebase, REST APIs, Coroutines, patrón Singleton, serialización...). Conecta con los módulos del ciclo formativo.]", { italic: true, color: "888888" }),
  p("[Párrafo 4 — Valoración personal: ¿Estás satisfecho con el resultado? ¿Qué harías diferente si tuvieras que volver a empezar?]", { italic: true, color: "888888" }),
  p("[Párrafo 5 — Cierre: Una reflexión sobre la experiencia de llevar un proyecto de principio a fin, desde el diseño hasta el ejecutable final.]", { italic: true, color: "888888" }),
  salto(),
];

// ── 6. BIBLIOGRAFÍA ──
const bibliografia = [
  new Paragraph({
    heading: HeadingLevel.HEADING_1,
    children: [new TextRun({ text: "6.  Bibliografía", font: FONT, size: 32, bold: true, color: AZUL })]
  }),
  separador(),
  new Paragraph({
    heading: HeadingLevel.HEADING_2,
    children: [new TextRun({ text: "Documentación oficial", font: FONT, size: 26, bold: true, color: "2E4057" })]
  }),
  balaNum("Unity Technologies. \"Unity Documentation 2022 LTS\". https://docs.unity3d.com/2022.3/Documentation/Manual/"),
  balaNum("Unity Technologies. \"Scripting Reference — MonoBehaviour\". https://docs.unity3d.com/ScriptReference/MonoBehaviour.html"),
  balaNum("Unity Technologies. \"TextMeshPro User Guide\". https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/"),
  balaNum("Google LLC. \"Firebase Realtime Database REST API\". https://firebase.google.com/docs/database/rest/"),
  balaNum("Google LLC. \"Identity Platform — REST API Reference\". https://cloud.google.com/identity-platform/docs/reference/rest"),
  balaNum("Google LLC. \"Firebase Security Rules\". https://firebase.google.com/docs/database/security"),
  balaNum("Microsoft. \"C# Programming Guide\". https://learn.microsoft.com/es-es/dotnet/csharp/"),
  balaNum("Microsoft. \"CultureInfo.InvariantCulture\". https://learn.microsoft.com/es-es/dotnet/api/system.globalization.cultureinfo.invariantculture"),
  new Paragraph({
    heading: HeadingLevel.HEADING_2,
    children: [new TextRun({ text: "Artículos y tutoriales", font: FONT, size: 26, bold: true, color: "2E4057" })]
  }),
  balaNum("Unity Technologies. \"Best practices: Singletons in Unity\". https://unity.com/how-to/use-design-patterns-to-organize-your-unity-project"),
  balaNum("Brackeys. \"SAVE & LOAD SYSTEM in Unity\". https://www.youtube.com/watch?v=XOjd_qU2Ido"),
  balaNum("Game Dev Beginner. \"Coroutines in Unity – A Beginner's Guide\". https://gamedevbeginner.com/coroutines-in-unity/"),
  new Paragraph({
    heading: HeadingLevel.HEADING_2,
    children: [new TextRun({ text: "Assets utilizados", font: FONT, size: 26, bold: true, color: "2E4057" })]
  }),
  p("[SUGERENCIA — Lista aquí los sprites, fuentes, música y otros assets de terceros que hayas usado, indicando su origen y licencia. Si son de creación propia, indícalo.]", { italic: true, color: "888888" }),
  salto(),
];

// ── ANEXOS ──
const anexos = [
  new Paragraph({
    heading: HeadingLevel.HEADING_1,
    children: [new TextRun({ text: "Anexo I — Capturas de pantalla", font: FONT, size: 32, bold: true, color: AZUL })]
  }),
  separador(),
  p("[SUGERENCIA — Inserta 6-10 capturas de la aplicación en funcionamiento: pantalla de login, pantalla de registro, juego principal con parcelas activas, tienda, mejoras, perfil con edición de nombre, configuración con slider, ranking con datos reales y, opcionalmente, una captura de la consola de Firebase.]", { italic: true, color: "888888" }),
  salto(),
  new Paragraph({
    heading: HeadingLevel.HEADING_1,
    children: [new TextRun({ text: "Anexo II — Diagrama de arquitectura", font: FONT, size: 32, bold: true, color: AZUL })]
  }),
  separador(),
  p("[SUGERENCIA — Inserta aquí el diagrama de arquitectura del sistema (puedes dibujarlo con draw.io en app.diagrams.net, exportarlo como PNG e insertarlo). Opcionalmente añade un diagrama de clases simplificado con las relaciones entre los 12 scripts.]", { italic: true, color: "888888" }),
];

// ─── DOCUMENTO ────────────────────────────────────────────────────────────────

const children = [
  ...portada,
  ...indice,
  ...introduccion,
  ...objetivos,
  ...faseDiseño,
  ...faseImplementacion,
  ...fasePruebas,
  ...ampliaciones,
  ...conclusion,
  ...bibliografia,
  ...anexos,
];

const doc = new Document({
  numbering: {
    config: [
      {
        reference: "bullets",
        levels: [{
          level: 0, format: LevelFormat.BULLET, text: "•",
          alignment: AlignmentType.LEFT,
          style: { paragraph: { indent: { left: 720, hanging: 360 } } }
        }]
      },
      {
        reference: "numbers",
        levels: [{
          level: 0, format: LevelFormat.DECIMAL, text: "[%1]",
          alignment: AlignmentType.LEFT,
          style: { paragraph: { indent: { left: 720, hanging: 440 } } }
        }]
      },
    ]
  },
  styles: {
    default: {
      document: { run: { font: FONT, size: 24 } }
    },
    paragraphStyles: [
      {
        id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 32, bold: true, font: FONT, color: AZUL },
        paragraph: { spacing: { before: 480, after: 200 }, outlineLevel: 0 }
      },
      {
        id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 26, bold: true, font: FONT, color: "2E4057" },
        paragraph: { spacing: { before: 320, after: 160 }, outlineLevel: 1 }
      },
      {
        id: "Heading3", name: "Heading 3", basedOn: "Normal", next: "Normal", quickFormat: true,
        run: { size: 24, bold: true, font: FONT, color: "4472C4" },
        paragraph: { spacing: { before: 240, after: 120 }, outlineLevel: 2 }
      },
    ]
  },
  sections: [{
    properties: {
      page: {
        size: { width: 11906, height: 16838 },
        margin: { top: 1440, right: 1440, bottom: 1440, left: 1800 }
      }
    },
    headers: {
      default: new Header({
        children: [new Paragraph({
          alignment: AlignmentType.RIGHT,
          border: { bottom: { style: BorderStyle.SINGLE, size: 4, color: "BFBFBF", space: 1 } },
          children: [new TextRun({ text: "RelaxingFarm — Memoria del Proyecto de Fin de Ciclo", font: FONT, size: 18, color: "808080" })]
        })]
      })
    },
    footers: {
      default: new Footer({
        children: [new Paragraph({
          alignment: AlignmentType.CENTER,
          border: { top: { style: BorderStyle.SINGLE, size: 4, color: "BFBFBF", space: 1 } },
          children: [
            new TextRun({ text: "[TU NOMBRE]  ·  ", font: FONT, size: 18, color: "808080" }),
            new TextRun({ children: [PageNumber.CURRENT], font: FONT, size: 18, color: "808080" }),
          ]
        })]
      })
    },
    children,
  }]
});

Packer.toBuffer(doc).then(buf => {
  fs.writeFileSync("C:\\Users\\hugom\\RelaxingFarmUnity\\Memoria_TFG_RelaxingFarm.docx", buf);
  console.log("✅  Documento generado: Memoria_TFG_RelaxingFarm.docx");
});
