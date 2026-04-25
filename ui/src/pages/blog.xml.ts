import rss from '@astrojs/rss';
import { getCollection } from 'astro:content';

export async function GET(context: any) {
  const posts = await getCollection('blog');
  return rss({
    title: 'Phaeno – Latest updates',
    description: 'Insights on RNA-sequencing and our PSeq platform',
    site: context.site,
    items: posts.map((post) => ({
      title: post.data.title,
      description: post.data.summary,
      pubDate: post.data.date,
      link: `/media/blog/${post.slug}/`,      // adjust route pattern if needed
    })),
    customData: `<language>en-us</language>`,
  });
}