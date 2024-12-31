using System;
using System.Collections;
using Npgsql;
using UnityEngine;

public class PostgreSQLDynamicQuery : MonoBehaviour
{
    // Datos de conexi�n proporcionados
    private string host = ""; // Direcci�n del servidor PostgreSQL
    private string port = ""; // Puerto de PostgreSQL
    private string dbName = ""; // Nombre de la base de datos
    private string username = ""; // Usuario de la base de datos
    private string password = ""; // Contrase�a de la base de datos

    private NpgsqlConnection connection;
    private bool isConnected = false;

    // Start is called before the first frame update
    void Start()
    {
        // Iniciar la conexi�n y la consulta continua
        StartCoroutine(QueryLoop());
    }

    // Coroutine para realizar la consulta cada 2 segundos
    IEnumerator QueryLoop()
    {
        // Establecer la conexi�n inicial
        if (!isConnected)
        {
            yield return StartCoroutine(ConnectToDatabase());
        }

        while (isConnected)
        {
            // Ejecutar la consulta
            ExecuteQuery();
            yield return new WaitForSeconds(2); // Esperar 2 segundos entre consultas
        }
    }

    // Conectar a la base de datos de PostgreSQL
    private IEnumerator ConnectToDatabase()
    {
        try
        {
            // Construir la cadena de conexi�n
            string connString = $"Host={host};Port={port};Username={username};Password={password};Database={dbName}";
            connection = new NpgsqlConnection(connString);
            connection.Open();
            isConnected = true;
            Debug.Log("Conexi�n exitosa a PostgreSQL.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error en la conexi�n a la base de datos: " + ex.Message);
            isConnected = false;
        }

        yield return null; // Finalizar la corrutina de conexi�n
    }

    // Ejecutar la consulta sobre la tabla "cultivos"
    private void ExecuteQuery()
    {
        if (connection == null || connection.State != System.Data.ConnectionState.Open)
        {
            Debug.LogError("Conexi�n a la base de datos no est� abierta.");
            return;
        }

        // Consulta para obtener todos los registros de la tabla 'cultivos'
        string query = "SELECT * FROM cultivos";
        using (var cmd = new NpgsqlCommand(query, connection))
        {
            try
            {
                // Ejecutar la consulta y leer los resultados
                using (var reader = cmd.ExecuteReader())
                {
                    // Obtener los nombres de las columnas
                    var columnNames = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        columnNames[i] = reader.GetName(i);  // Nombre de cada columna
                    }

                    // Leer cada fila y mostrar los resultados din�micamente
                    while (reader.Read())
                    {
                        string rowOutput = "Registro: ";

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            // Obtener el valor de la columna dependiendo de su tipo
                            var columnValue = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString();

                            rowOutput += $"{columnNames[i]}: {columnValue} ";
                        }

                        // Mostrar el registro completo en la consola
                        Debug.Log(rowOutput);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error al ejecutar la consulta: " + ex.Message);
            }
        }
    }

    // Asegurarse de cerrar la conexi�n cuando la aplicaci�n termine
    void OnApplicationQuit()
    {
        if (connection != null && connection.State == System.Data.ConnectionState.Open)
        {
            connection.Close();
            Debug.Log("Conexi�n cerrada al salir de la aplicaci�n.");
        }
    }
}

