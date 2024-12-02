# Task Manager API

Task Manager API est une application backend en C# qui permet de gérer des tâches, des utilisateurs et des commentaires. Cette API propose des fonctionnalités pour créer, lire, mettre à jour et supprimer des tâches, tout en offrant des options avancées comme l'exportation en CSV et des statistiques.

## Fonctionnalités

- **Gestion des tâches** : CRUD (Créer, Lire, Mettre à jour, Supprimer) des tâches.
- **Gestion des utilisateurs** : CRUD des utilisateurs et attribution des tâches à des utilisateurs.
- **Gestion des commentaires** : CRUD des commentaires liés aux tâches.
- **Exportation en CSV** : Export des tâches au format CSV.
- **Statistiques** : Récupération des statistiques globales sur les tâches et les utilisateurs.

---

## Installation et configuration

### Prérequis

- .NET 8.0 ou supérieur
- SQLite
- Visual Studio ou Visual Studio Code

### Étapes d'installation

1. Clonez le dépôt :

   ```bash
   git clone https://github.com/votre-utilisateur/task-manager-api.git
   cd task-manager-api
   ```

2. Configurez la base de données SQLite :

- La configuration SQLite est déjà incluse dans le fichier ``appsettings.json``. Voici un exemple de configuration :

Exemple dans ``appsettings.json`` :

    {
    "ConnectionStrings": {
        "DefaultConnection": "Data Source=TaskManagerDB.sqlite"
        }
    }

3. Appliquez les migrations pour créer la base de données SQLite (je précise qu'il faut entity framework pour que tout fonctionne pour les migrations etc..) :

    ``dotnet ef database update``

4. Lancez l'application :

    ``dotnet run``

5. Accédez à Swagger pour tester l'API :

Ouvrez un navigateur et rendez-vous à l'adresse suivante : http://localhost:5051/swagger.


## Endpoints principaux
### Tâches
1. Créer une tâche
 - Méthode : POST
 - URL : ``/api/Task``
 - Exemple de body :
```
{
    "id": 0,
    "title": "Task Title",
    "description": "Task Description",
    "status": "En attente",
    "createdAt": "2024-12-02T20:03:36.323Z",
    "userId": 1,
    "comments": []
}

```



2. Récupérer toutes les tâches
Méthode : GET
URL : ``/api/Task``
Query Parameters :
status : (optionnel) Filtrer par statut.

3. Mettre à jour une tâche
Méthode : PUT
URL : ``/api/Task/{id}``
Exemple de body :
```
{
  "id": 1,
  "title": "Titre mis à jour",
  "description": "Nouvelle description",
  "status": "Terminée",
  "userId": 1
}
```

4. Supprimer une tâche
Méthode : DELETE
URL : ``/api/Task/{id}``

5. Exporter les tâches en CSV
Méthode : GET
URL : ``/api/Task/export``
Réponse : Fichier CSV contenant toutes les tâches.
### Utilisateurs
1. Créer un utilisateur
Méthode : POST
URL : ``/api/Users``
Exemple de body :
```
{
  "name": "John Doe"
}
```
2. Assigner une tâche à un utilisateur
Méthode : POST
URL :`` /api/Task/assign/{taskId}/{userId}``

Réponse :
```
{
  "$id": "1",
  "message": "Task assigned successfully",
  "task": {
    "$id": "2",
    "id": 9,
    "title": "Titre mis à jour",
    "description": "Nouvelle description",
    "status": "Terminée",
    "createdAt": "2024-12-02T22:11:13.2968575",
    "userId": 2,
    "assignedUser": {
      "$id": "3",
      "id": 2,
      "name": "stone",
      "tasks": {
        "$id": "4",
        "$values": [
          {
            "$ref": "2"
          }
        ]
      }
    },
    "comments": null
  }
}
```

### Commentaires
1. Ajouter un commentaire
Méthode : POST
URL : ``/api/Comments``

Exemple de body :
```
{
  "id": 0,
  "content": "string",
  "taskId": 0,
  "userId": 0,
  "createdAt": "2024-12-02T22:13:39.467Z",
}
```
2. Récupérer les commentaires d'une tâche
Méthode : GET
URL : ``/api/Comments/Task/{taskId}``

Reponse : 
```
{
  "$id": "1",
  "$values": [
    {
      "$id": "2",
      "id": 6,
      "content": "string",
      "taskId": 5,
      "userId": 2,
      "createdAt": "2024-12-02T21:37:46.648",
      "user": null
    },
    {
      "$id": "3",
      "id": 7,
      "content": "string",
      "taskId": 5,
      "userId": 2,
      "createdAt": "2024-12-02T21:37:46.648",
      "user": null
    }
  ]
}
```

3. Ajouter un commentaire
Méthode : POST
URL : ``/api/Comments/{id}``

Exemple de body :
```
{
  "id": 0,
  "content": "string",
  "taskId": 0,
  "userId": 0,
  "createdAt": "2024-12-02T22:13:39.467Z",
}
```