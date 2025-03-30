

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;


public class Program
{
    public static void Main(string[] args)
    {
        // Création des utilisateurs
        var client = new Client(1, "Jean Dupont", "jean@example.com", "motdepasse123");
        var vendeur = new Vendeur(2, "Marie Martin", "marie@example.com", "motdepasse456");

        // Importation des fleurs depuis un fichier CSV
        var gestionFleurs = new GestionFleurs();
        gestionFleurs.ImporterFleursDepuisCSV();

        if (gestionFleurs.Fleurs.Count == 0)
        {
            Console.WriteLine("Aucune fleur n'a été importée depuis le fichier CSV.");
            return;  // Arrête l'exécution si aucune fleur n'a été importée
        }

        // Création d'une commande
        var fleursCommandees = new List<Fleur> { gestionFleurs.Fleurs[0], gestionFleurs.Fleurs[1] }; // Exemple avec deux fleurs
        var commande = new Commande(1, client, fleursCommandees, "Carte de crédit");
        commande.CalculerMontantTotal();
        commande.AfficherDetailsCommande();

        // Passage de la commande
        vendeur.PasserCommande(commande);
        
        // Génération de la facture
        var facture = new Facture(commande);
        facture.GenererFacture();
    }
}

public abstract class Utilisateur
{
    public int Id { get; set; }
    public string Nom { get; set; }
    public string Email { get; set; }
    public string MotDePasse { get; set; }
    public string Type { get; set; }

    protected Utilisateur(int id, string nom, string email, string motDePasse, string type)
    {
        Id = id;
        Nom = nom;
        Email = email;
        MotDePasse = motDePasse;
        Type = type;
    }

    public abstract void EffectuerTache();
}

public interface ICommande
{
    void PasserCommande(Commande commande);
    void SuivreCommande(int idCommande);
    void PayerCommande(int idCommande);
}

public class Fleur
{
    public int Id { get; set; }
    public string Nom { get; set; }
    public string Couleur { get; set; }
    public decimal Prix { get; set; }
    public string Description { get; set; }

    public Fleur(int id, string nom, string couleur, decimal prix, string description)
    {
        Id = id;
        Nom = nom;
        Couleur = couleur;
        Prix = prix;
        Description = description;
    }
}
public class GestionFleurs
{
    public List<Fleur> Fleurs { get; set; } = new List<Fleur>();
    private int idAutoIncrement = 1;

    public void ImporterFleursDepuisCSV()
    {
        var cheminFichier = "fleurs_db.csv";
        try
        {
            if (!File.Exists(cheminFichier))
            {
                Console.WriteLine("Le fichier fleurs_db.csv n'existe pas.");
                return;
            }

            var lignes = File.ReadAllLines(cheminFichier);
            
            Console.WriteLine("Contenu du fichier CSV :");
            foreach (var ligne in lignes)
            {
                Console.WriteLine(ligne);
            }

            if (lignes.Length <= 1)
            {
                Console.WriteLine("Aucune fleur n'a été trouvée dans le fichier.");
                return;
            }

            foreach (var ligne in lignes.Skip(1)) // Ignorer la première ligne (les en-têtes)
            {
                var donnees = ligne.Split(',');
                if (donnees.Length == 4)  
                {
                    try
                    {
                        // Utiliser CultureInfo.InvariantCulture pour garantir la bonne conversion
                        var prix = decimal.Parse(donnees[1], CultureInfo.InvariantCulture);

                        var fleur = new Fleur(
                            idAutoIncrement++,
                            donnees[0], 
                            donnees[2], 
                            prix, 
                            donnees[3]
                        );
                        Fleurs.Add(fleur);
                        Console.WriteLine($"Fleur ajoutée : {fleur.Nom}, {fleur.Couleur}, {fleur.Prix} $"); // Afficher que la fleur a été ajoutée
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine($"Erreur lors de la conversion du prix pour la fleur : {donnees[0]}");
                    }
                }
                else
                {
                    Console.WriteLine($"Ligne invalide dans le fichier CSV : {ligne}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur d'importation : {ex.Message}");
        }
    }
}
public class Bouquet
{
    public string Nom { get; set; }
    public List<Fleur> Fleurs { get; set; }

    public Bouquet(string nom, List<Fleur> fleurs)
    {
        Nom = nom;
        Fleurs = fleurs;
    }

    public decimal CalculerPrix()
    {
        decimal prixTotal = Fleurs.Sum(f => f.Prix) + 2m + 1m; // 2$ pour le labeur, 1$ pour la carte
        return prixTotal;
    }
}
public class Commande
{
    public int Id { get; set; }
    public Client Client { get; set; }
    public List<Fleur> Fleurs { get; set; }
    public string ModePaiement { get; set; }
    public decimal MontantTotal { get; set; }
    public DateTime DateCommande { get; set; }
    public string Statut { get; set; }

    public Commande(int id, Client client, List<Fleur> fleurs, string modePaiement)
    {
        Id = id;
        Client = client;
        Fleurs = fleurs;
        ModePaiement = modePaiement;
        DateCommande = DateTime.Now;
        Statut = "En traitement";
    }

    public void CalculerMontantTotal()
    {
        MontantTotal = Fleurs.Sum(f => f.Prix) + 3m; // 3$ frais supplémentaires
    }

    public void AfficherDetailsCommande()
    {
        Console.WriteLine($"Commande ID: {Id}");
        Console.WriteLine($"Client: {Client.Nom}");
        Console.WriteLine($"Date: {DateCommande.ToShortDateString()}");
        Console.WriteLine($"Montant total: {MontantTotal} $");
        Console.WriteLine($"Mode de paiement: {ModePaiement}");
        foreach (var fleur in Fleurs)
        {
            Console.WriteLine($"Fleur: {fleur.Nom}, Prix: {fleur.Prix}");
        }
    }
}

public class Facture
{
    public Commande Commande { get; set; }

    public Facture(Commande commande)
    {
        Commande = commande;
    }

    public void GenererFacture()
    {
        Console.WriteLine("\n--- Facture ---");
        Console.WriteLine($"ID Commande: {Commande.Id}");
        Console.WriteLine($"Client: {Commande.Client.Nom}");
        Console.WriteLine($"Montant Total: {Commande.MontantTotal} $");
        Console.WriteLine($"Mode de Paiement: {Commande.ModePaiement}");
        Console.WriteLine("Fleurs commandées:");
        foreach (var fleur in Commande.Fleurs)
        {
            Console.WriteLine($"- {fleur.Nom} ({fleur.Prix} $)");
        }
    }
}
public class Client : Utilisateur, ICommande
{
    public List<Commande> Commandes { get; set; } = new List<Commande>();

    public Client(int id, string nom, string email, string motDePasse)
        : base(id, nom, email, motDePasse, "Client")
    { }

    public override void EffectuerTache()
    {
        Console.WriteLine($"Le client {Nom} passe une commande.");
    }

    public void PasserCommande(Commande commande)
    {
        Commandes.Add(commande);
        Console.WriteLine($"Commande {commande.Id} passée.");
    }

    public void SuivreCommande(int idCommande)
    {
        Console.WriteLine($"Suivi de la commande {idCommande}.");
    }

    public void PayerCommande(int idCommande)
    {
        Console.WriteLine($"Commande {idCommande} payée.");
    }
}

public class Proprietaire : Utilisateur
{
    public string Magasin { get; set; }

    public Proprietaire(int id, string nom, string email, string motDePasse, string magasin)
        : base(id, nom, email, motDePasse, "Proprietaire")
    {
        Magasin = magasin;
    }

    public override void EffectuerTache()
    {
        Console.WriteLine($"Le propriétaire {Nom} gère le magasin {Magasin}.");
    }
}

public class Vendeur : Utilisateur, ICommande
{
    public List<Commande> Commandes { get; set; } = new List<Commande>();

    public Vendeur(int id, string nom, string email, string motDePasse)
        : base(id, nom, email, motDePasse, "Vendeur")
    { }

    public override void EffectuerTache()
    {
        Console.WriteLine($"Le vendeur {Nom} traite des commandes.");
    }

    public void PasserCommande(Commande commande)
    {
        Commandes.Add(commande);
        Console.WriteLine($"Commande {commande.Id} passée par le vendeur {Nom}.");
    }

    public void SuivreCommande(int idCommande)
    {
        Console.WriteLine($"Suivi de la commande {idCommande}.");
    }

    public void PayerCommande(int idCommande)
    {
        Console.WriteLine($"Commande {idCommande} payée.");
    }
}

public class Fournisseur : Utilisateur, ICommande
{
    public List<Fleur> Fleurs { get; set; } = new List<Fleur>();

    public Fournisseur(int id, string nom, string email, string motDePasse)
        : base(id, nom, email, motDePasse, "Fournisseur")
    { }

    public override void EffectuerTache()
    {
        Console.WriteLine($"Le fournisseur {Nom} fournit des fleurs.");
    }

    public void PasserCommande(Commande commande)
    {
        Console.WriteLine($"Commande de fleurs {commande.Id} passée au fournisseur {Nom}.");
    }

    public void SuivreCommande(int idCommande)
    {
        Console.WriteLine($"Suivi de la commande {idCommande} par le fournisseur {Nom}.");
    }

    public void PayerCommande(int idCommande)
    {
        Console.WriteLine($"Paiement de la commande {idCommande} effectué par le fournisseur.");
    }
}
