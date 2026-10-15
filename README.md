#Akatsuki Tarea Corta 2 - API Gateway con Microservicios

Curso: Programación V Institución: Colegio Universitario de Cartago Profesor: Profesor Ingeniero Gabriel González Solano Grupo: Akatsukis Fecha de entrega: 15 de octubre de 2026

Descripción

Este repositorio contiene la parte práctica de la Tarea Corta 2 sobre API Gateway Se implementan dos microservicios REST con persistencia en MongoDB y un API Gateway construido con YARP que funciona como entrada única para quien consuma el sistema Además del enrutamiento, el Gateway aplica transformación de encabezados, agrega datos de ambos servicios en un solo endpoint y registra cada solicitud que recibe

Integrantes y responsabilidades

Sebastián Jiménez Arrieta: Microservicio de Productos Sebastián Obando Ramírez: Microservicio de Pedidos Brandon Guido Navarro: API Gateway e integración de los microservicios Anthony Gamboa Elizondo: video con la explicación de todo lo implementado Julián Solano Obando: investigación formal de los conceptos solicitados

Arquitectura

El cliente solo habla con el Gateway, y el Gateway decide a cuál servicio enviar cada solicitud

Cliente (Postman) -> API Gateway en el puerto 5262 API Gateway -> Microservicio de Productos en el puerto 5063 API Gateway -> Microservicio de Pedidos en el puerto 5282 Microservicio de Pedidos -> Microservicio de Productos, para validar que el producto exista antes de crear un pedido Cada microservicio guarda sus datos en su propia base de datos de MongoDB

Tecnologías utilizadas

SDK de dotnet 10 ASP NET Core con Minimal APIs YARP como reverse proxy del Gateway MongoDB como base de datos No SQL Swagger en el microservicio de Productos OpenAPI en el Gateway y en el microservicio de Pedidos

Estructura del repositorio

Documentacion: investigación en PDF, asignación de responsabilidades en Excel y la consigna de la tarea Evidencia_Pruebas_Tecnicas: documento con las capturas de las pruebas y el enlace a la grabación en video TareaAPIGateway/APIGateway: el Gateway TareaAPIGateway/MicroservicioProducto: el servicio de catálogo TareaAPIGateway/MicroservicioPedido: el servicio de órdenes

Cada servicio sigue la misma organización por capas Endpoints recibe las peticiones HTTP Services contiene la lógica de negocio Repository se encarga del acceso a MongoDB Entities define los modelos

Requisitos previos

SDK de dotnet 10 instalado MongoDB corriendo en el equipo en el puerto 27017 Postman para probar los endpoints Git para clonar el repositorio

Para verificar las versiones instaladas se puede usar

dotnet --version
mongod --version
Instalación

Clonar el repositorio y entrar a la carpeta de la solución

git clone https://github.com/H-K-12/Akatsuki_Tarea_2
cd Akatsuki_Tarea_2/TareaAPIGateway
dotnet restore

No hace falta crear las bases de datos a mano, MongoDB las crea al guardar el primer dato

Configuración

Los valores por defecto funcionan sin cambios si MongoDB corre de forma local con el puerto estándar

Microservicio de Productos Cadena de conexión: mongodb://localhost:27017/ Base de datos: ProductosDB Colección: Productos

Microservicio de Pedidos Cadena de conexión: mongodb://localhost:27017 Base de datos: AkatsukiPedidos Colección: Pedidos Dirección del servicio de Productos: http://localhost:5063/

API Gateway Dirección de Productos: http://localhost:5063/ Dirección de Pedidos: http://localhost:5282/

Estos valores están en el archivo de configuración appsettings de cada proyecto

Ejecución

Cada servicio corre de forma independiente en su propia terminal El orden recomendado es Productos, luego Pedidos y al final el Gateway Todos se levantan con el perfil http

Terminal 1, microservicio de Productos

cd TareaAPIGateway/MicroservicioProducto
dotnet run --launch-profile http

Terminal 2, microservicio de Pedidos

cd TareaAPIGateway/MicroservicioPedido
dotnet run --launch-profile http

Terminal 3, API Gateway

cd TareaAPIGateway/APIGateway
dotnet run --launch-profile http

También se puede abrir la solución en Visual Studio, configurar los tres proyectos como proyectos de inicio múltiple y ejecutarlos juntos

Puertos

API Gateway: 5262 Microservicio de Productos: 5063 Microservicio de Pedidos: 5282

El cliente debe usar siempre el puerto 5262, los otros dos puertos son internos

Endpoints expuestos por el Gateway

Todas las rutas se consumen en http://localhost:5262

Productos POST /api/productos para crear un producto con nombre y precio GET /api/productos para listar todos los productos GET /api/productos/{id} para obtener un producto por su identificador

Órdenes POST /api/ordenes para crear un pedido con productoId y cantidad GET /api/ordenes para listar todos los pedidos GET /api/ordenes/{id} para obtener un pedido por su identificador

Agregación GET /api/resumen-orden/{id} devuelve la orden junto con el detalle de sus productos

Salud GET /health indica si el Gateway está disponible

Ejemplos de uso

Crear un producto

POST http://localhost:5262/api/productos
Content-Type: application/json

{
  "nombre": "Teclado mecánico",
  "precio": 45000
}

La respuesta incluye el identificador generado por MongoDB, que se usa en el siguiente paso

Crear un pedido

POST http://localhost:5262/api/ordenes
Content-Type: application/json

{
  "productoId": "IDENTIFICADOR_DEL_PRODUCTO",
  "cantidad": 2
}

Consultar el resumen de la orden

GET http://localhost:5262/api/resumen-orden/IDENTIFICADOR_DEL_PEDIDO
Reglas de validación

Productos El nombre es obligatorio y debe tener entre 2 y 100 caracteres El precio debe ser mayor a cero Un producto inexistente responde 404

Pedidos El producto debe existir en el servicio de Productos, de lo contrario se responde 400 con un mensaje claro La cantidad debe ser mayor a cero Si el servicio de Productos no está disponible se responde 503 Un identificador con formato inválido responde 400 y un pedido inexistente responde 404

Resumen de orden Si el servicio de Pedidos responde con error, el Gateway propaga ese código Si algún microservicio está caído responde 503 Si algún microservicio tarda demasiado responde 504 Si algún microservicio devuelve un JSON inválido responde 502

Capacidades del Gateway

Entrada única: un solo puerto para todo el sistema Routing: las rutas de productos y ordenes se envían al servicio que corresponde Transformación de encabezados: el Gateway agrega el encabezado X-Gateway con el valor Akatsuki-ApiGateway tanto en la solicitud que va al microservicio como en la respuesta que recibe el cliente Agregación: el endpoint de resumen de orden consulta primero a Pedidos y luego a Productos para armar una sola respuesta Correlación: cada solicitud recibe un encabezado X-Correlation-ID, se reutiliza el que envía el cliente o se genera uno nuevo Tiempos límite: 30 segundos para el proxy y 10 segundos para las llamadas de agregación

Observabilidad

Gateway: registra cada solicitud recibida con método y ruta, y al terminar registra el código de estado y la duración en milisegundos, junto con el identificador de correlación YARP agrega sus propios registros del enrutamiento hacia cada servicio Microservicio de Pedidos: registra cada solicitud recibida y cada respuesta enviada, además de la creación de cada pedido Todos los registros se ven en la consola de cada servicio

Cumplimiento de la consigna

Dos microservicios con API REST propia: cumplido Persistencia en base de datos No SQL: cumplido con MongoDB Cada servicio en un puerto distinto y ejecutable de forma independiente: cumplido Comunicación entre servicios: Pedidos consulta a Productos al crear un pedido Entrada única, routing y dos políticas adicionales: cumplido con transformación de encabezados y agregación Servicio opcional de Clientes: no se implementó, por lo tanto la ruta de clientes no aplica

Documentación y evidencias

Documentacion/1 Investigacion Gateways, documento PDF con la parte teórica Documentacion/2 Asignacion de Responsabilidades, hoja de Excel con la división del trabajo Evidencia_Pruebas_Tecnicas, documento de evidencia con las pruebas en Postman y el archivo con el enlace al video de la demostración

Notas

El servicio de Clientes quedó fuera del alcance por ser opcional El servicio de Pedidos guarda un producto por pedido, con su identificador y la cantidad
[Razón del Repositorio](<Documentacion/README.md>)