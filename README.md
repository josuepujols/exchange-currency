# Exchange Currency 

---

## Tabla de Contenidos

- [Descripción](#descripción)
- [Características](#características)
- [Tecnologías](#NET, ASP.NET Core, MSTest, Docker)
- [Instalación](#DockerContainer)
- [Uso](#docker run -d -p 5500:5500 --name exchange-currency-container exchange-currency.1)
- [Contribuciones](#Josue Pujols)
- [Contacto](#josuepujols20@icloud.com - 829-381-1528)

---

## Descripción

Proyecto que consiste en consultar 3 servicios externos con diferentes estructuras/prot para conversion de monedas de los cuales dependiendo de la taza mas alta que nos brinde uno de los 3 la escogemos para devolverla como resultado.

---

## Características

- Consulta 3 diferentes API's para obetenr el mejor rate, basado en el rate mas alto para la taza de cambio devolvemos esa respuesta con su respectivo proveedor 
- Tambien en la respuesta devolvemos el tiempo que se demoro ese proveedor para dar la respuesta para tambien evaluar esa parte.
---

## Tecnologías

NET, ASP.NET Core, MSTest, Docker

---

## Instalación

Dentro del proyecto cuando lo clonemos encontraremos un docker file, solo tendremos que hacer build de la iamgen y luego correr el contenedor para probar.

```bash
# Ejemplo para clonar el repositorio
git clone https://github.com/usuario/proyecto.git](https://github.com/josuepujols/exchange-currency.git)
cd ExchangeCurrrency

# Build de la imagen
docker build --no-cache -t exchange-currency.1 . --progress=plain

# Corremos el contenedor (Nota: el puerto para el contenedor es el 5500)
docker run -d -p 5500:5500 --name exchange-currency-container exchange-currency.1

# Podemos usar el servicio, hacemos un POST request a http://localhost:5500/api/Exchange\
REQUEST
curl -X POST http://localhost:5500/api/Exchange \
  -H "Content-Type: application/json" \
  -d '{
    "fromCurrency": "EUR",
    "toCurrency": "USD",
    "amount": 1000
  }'

RESPONSE
{
  "provider": "Api2",
  "rate": 1164.8000,
  "latencyMs": 145
}
