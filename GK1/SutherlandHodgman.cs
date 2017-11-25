using GK1;
using System;
using System.Collections.Generic;

using static GK1.Geometry;

namespace GK1
{
    public static class SutherlandHodgman
    {
        /// <summary>
        /// Oblicza pole wielokata przy pomocy formuly Gaussa
        /// </summary>
        /// <param name="polyline">Kolejne wierzcholki wielokata</param>
        /// <returns>Pole wielokata</returns>
        public static double PolylineArea(this GKPolyline polyline)
        {
            double result = 0;
            for (int i = 0; i < polyline.vertices.Count - 1; i++)
            {
                result += Vertice.CrossProduct(polyline.vertices[i] - polyline.vertices[0], polyline.vertices[i + 1] - polyline.vertices[0]);
            }
            return Math.Abs(result) / 2;
        }

        /// <summary>
        /// Sprawdza, czy dwa punkty leza po tej samej stronie prostej wyznaczonej przez odcinek s
        /// </summary>
        /// <param name="p1">Pierwszy punkt</param>
        /// <param name="p2">Drugi punkt</param>
        /// <param name="s">Odcinek wyznaczajacy prosta</param>
        /// <returns>
        /// True, jesli punkty leza po tej samej stronie prostej wyznaczonej przez odcinek 
        /// (lub co najmniej jeden z punktow lezy na prostej). W przeciwnym przypadku zwraca false.
        /// </returns>
        public static bool IsSameSide(Vertice p1, Vertice p2, Edge s)
        {
            Vertice refer = s.v2;
            Vertice refer2 = s.v1;

            double val1 = Vertice.CrossProduct(refer2 - refer, p1 - refer);
            double val2 = Vertice.CrossProduct(refer2 - refer, p2 - refer);

            if (val1 == 0 || val2 == 0) return true;
            if (val1 * val2 < 0) return false;
            return true;
        }

        /// <summary>Oblicza czesc wspolna dwoch wielokatow przy pomocy algorytmu Sutherlanda–Hodgmana</summary>
        /// <param name="subjectPolyline">Wielokat obcinany (wklesly lub wypukly)</param>
        /// <param name="clipPolyline">Wielokat obcinajacy (musi byc wypukly i zakladamy, ze taki jest)</param>
        /// <returns>Czesc wspolna wielokatow</returns>
        /// <remarks>
        /// - mozna zalozyc, ze 3 kolejne punkty w kazdym z wejsciowych wielokatow nie sa wspolliniowe
        /// - wynikiem dzialania funkcji moze byc tak naprawde wiele wielokatow (sytuacja taka moze wystapic,
        ///   jesli wielokat obcinany jest wklesly)
        /// - jesli wielokat obcinany i obcinajacy zawieraja wierzcholki o tych samych wspolrzednych,
        ///   w wynikowym wielokacie moge one byc zduplikowane
        /// - wierzcholki wielokata obcinanego, przez ktore przechodza krawedzie wielokata obcinajacego
        ///   zostaja zduplikowane w wielokacie wyjsciowym
        /// </remarks>
        public static GKPolyline GetIntersectedPolyline(GKPolyline subjectPolyline, GKPolyline clipPolyline)
        {
            //output = lista wierzcholkow wielokata obcinanego
            //foreach (krawedz e wielokata obcinajacego )
            //    {
            //                input = output
            //    output = pusta lista
            //    pp = ostatni element z input
            //    foreach (punkt p z input )
            //        {
            //           if (p jest po wewnetrznej stronie krawedzi e )
            //           {
            //               if (pp nie jest po wewnetrznej stronie krawedzi e )
            //                      dodaj punkt przeciecia odcinka < pp,p > i krawedzi e do  output
            //                dodaj punkt p output
            //           }
            //        else
            //           if (pp jest po wewnetrznej stronie krawedzi e )
            //              dodaj punkt przeciecia odcinka < pp,p > i krawedzi e do
            //                        output
            //pp = p
            //        }
            //            }
            //usunąć duplikaty z output // tylko dla wersji z usuwaniem duplikatow
            List<Edge> clipSegments = new List<Edge>();
            Vertice clipCenter = new Vertice();
            for (int i = 0; i < clipPolyline.vertices.Count; i++)
            {
                clipCenter.coords.X += clipPolyline.vertices[i].coords.X;
                clipCenter.coords.Y += clipPolyline.vertices[i].coords.Y;
                clipSegments.Add(new Edge(clipPolyline.vertices[i], clipPolyline.vertices[(i + 1) % clipPolyline.vertices.Count]));
            }
            clipCenter.coords.X /= clipPolyline.vertices.Count;
            clipCenter.coords.Y /= clipPolyline.vertices.Count;

            List<Vertice> output = new List<Vertice>(subjectPolyline.vertices);
            List<Vertice> input = new List<Vertice>();
            foreach (Edge clipSeg in clipSegments)
            {
                input = output;
                output = new List<Vertice>();
                Vertice pp = new Vertice();
                if (input.Count != 0) pp = input[input.Count - 1];
                foreach (Vertice p in input)
                {
                    if (IsSameSide(clipCenter, p, clipSeg))
                    {
                        if (!IsSameSide(clipCenter, pp, clipSeg))
                        {
                            output.Add(GetIntersectionPoint(new Edge(pp, p), clipSeg));
                        }
                        output.Add(p);
                    }
                    else
                    {
                        if (IsSameSide(clipCenter, pp, clipSeg))
                        {
                            output.Add(GetIntersectionPoint( new Edge(pp, p), clipSeg));
                        }
                    }
                    pp = p;
                }

            }
            List<Vertice> noDup = new List<Vertice>();
            foreach (Vertice p in output)
            {
                if (!noDup.Contains(p)) noDup.Add(p);
            }
            subjectPolyline.vertices = noDup;
            subjectPolyline.PopulateEdges();
            return subjectPolyline;
        }

        /// <summary>
        /// Zwraca punkt przeciecia dwoch prostych wyznaczonych przez odcinki
        /// </summary>
        /// <param name="seg1">Odcinek pierwszy</param>
        /// <param name="seg2">Odcinek drugi</param>
        /// <returns>Punkt przeciecia prostych wyznaczonych przez odcinki</returns>
        public static Vertice GetIntersectionPoint(Edge seg1, Edge seg2)
        {
            Vertice direction1 = new Vertice(seg1.v2.coords.X - seg1.v1.coords.X, seg1.v2.coords.Y - seg1.v1.coords.Y);
            Vertice direction2 = new Vertice(seg2.v2.coords.X - seg2.v1.coords.X, seg2.v2.coords.Y - seg2.v1.coords.Y);
            double dotPerp = (direction1.coords.X * direction2.coords.Y) - (direction1.coords.Y * direction2.coords.X);

            Vertice c = new Vertice(seg2.v1.coords.X - seg1.v1.coords.X, seg2.v1.coords.Y - seg1.v1.coords.Y);
            double t = (c.coords.X * direction2.coords.Y - c.coords.Y * direction2.coords.X) / dotPerp;

            return new Vertice(seg1.v1.coords.X + (t * direction1.coords.X), seg1.v1.coords.Y + (t * direction1.coords.Y));
        }
    }
}
